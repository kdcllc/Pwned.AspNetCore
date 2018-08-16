using System;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Pwned.AspNetCore
{
    ///<inheritdoc/>
    public class PwnedPasswordService : IPwnedPasswordService
    {
        ///<inheritdoc/>
        public HttpClient Client { get; }

        private readonly string _getRange = "range";
        private readonly PwnedOptions _options;

        /// <summary>
        /// Constructor for <see cref="PwnedPasswordService"/>.
        /// </summary>
        /// <param name="httpClient"><see cref="HttpClient"/> instance passed by DI injection</param>
        /// <param name="options"><see cref="IOptions{PwnedOptions}"/> instance passed by DI injection.</param>
        public PwnedPasswordService(HttpClient httpClient,
            IOptions<PwnedOptions> options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            Client = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _options = options.Value;

            var userAgent = $"{nameof(PwnedPasswordService)}-kdcllc";
            if (!string.IsNullOrEmpty(_options?.UserAgent))
            {
                userAgent = _options?.UserAgent;
            }

            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            httpClient.DefaultRequestHeaders.Add("User-Agent", userAgent);
        }

        ///<inheritdoc/>
        public async Task<(bool pwned, long count)> IsPasswordPwned(string password,
            CancellationToken token = default)
        {
            // Compute the SHA1 hash of the string
            var sha1 = SHA1.Create();
            var byteString = Encoding.UTF8.GetBytes(password);
            var hashBytes = sha1.ComputeHash(byteString);
            var hashString = "";

            var sb = new StringBuilder();
            foreach (var b in hashBytes)
            {
                sb.Append(b.ToString("X2"));
            }
            hashString = sb.ToString();

            // Break the SHA1 into two pieces:
            //   1) the first five characters of the hash
            //   2) the rest of the hash
            var hashFirstFive = hashString.Substring(0, 5);
            var hashLeftover = hashString.Substring(5, hashString.Length - 5);

            //GET https://api.pwnedpasswords.com/range/{first 5 hash chars}
            var url = $"{_options.PasswordsApiUrl}/{_getRange}/{hashFirstFive}";

            var request = new HttpRequestMessage(HttpMethod.Get, url);

            var response = await Client.SendAsync(request, token).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var responseContainsHash = result.Contains(hashLeftover);

            long breachCounts = 0;

            if (responseContainsHash)
            {
                var lines = result.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None).ToList();
                foreach (var item in lines)
                {
                    if (item.Contains(hashLeftover))
                    {
                        var records = item.Split(':');
                        long.TryParse(records[1], out var x);
                        breachCounts += x;
                    }
                }
            }
            return (responseContainsHash, breachCounts);
        }
    }
}
