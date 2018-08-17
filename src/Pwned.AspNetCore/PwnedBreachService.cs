using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pwned.AspNetCore.Models;

namespace Pwned.AspNetCore
{
    /// <summary>
    /// Simple <see cref="HttpClient"/> based wrapper around <see cref="!:https://haveibeenpwned.com/API/"/>
    /// </summary>
    public class PwnedBreachService : IPwnedBreachService
    {
        private readonly PwnedOptions _options;
        private readonly string _getBreachesByAccountUri = "breachedaccount";
        private readonly string _getAllBreachesUri = "breaches";
        private readonly string _getAllDataClasses = "dataclasses";
        private readonly string _getPastAccount = "pasteaccount";

        ///<inheritdoc/>
        public HttpClient Client { get; private set; }

        /// <summary>
        /// Constructor for <see cref="PwnedBreachService"/>.
        /// </summary>
        /// <param name="httpClient"><see cref="HttpClient"/> instance passed by DI injection</param>
        /// <param name="options"><see cref="IOptions{PwnedOptions}"/> instance passed by DI injection.</param>
        public PwnedBreachService(HttpClient httpClient,
            IOptions<PwnedOptions> options)
        {
            if (options == null)
            {
                throw new System.ArgumentNullException(nameof(options));
            }

            _options = options.Value;

            Client = httpClient ?? throw new System.ArgumentNullException(nameof(httpClient));

            var userAgent = $"{nameof(PwnedBreachService)}-kdcllc";
            if (!string.IsNullOrEmpty(_options?.UserAgent))
            {
                userAgent = _options?.UserAgent;
            }

            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            httpClient.DefaultRequestHeaders.Add("User-Agent", userAgent);
            httpClient.DefaultRequestHeaders.Add("api-version", _options.ServiceApiVersion);
        }

        ///<inheritdoc/>
        public Task<List<Breach>> GetBreachesByAccountAsync(string emailAccount,
            bool unVerified = false,
            bool fullResponse = true,
            string domain = "",
            CancellationToken token = default)
        {
            var args = new Dictionary<string, string>();

            //https://haveibeenpwned.com/api/v2/breachedaccount/test@example.com
            var url = $"{_options.ServiceApiUrl}/{_getBreachesByAccountUri}/{WebUtility.UrlEncode(emailAccount)}";

            if (unVerified)
            {
                //https://haveibeenpwned.com/api/v2/breachedaccount/test@example.com?includeUnverified=true
                args.Add("includeUnverified", unVerified.ToString());
            }

            if (!fullResponse)
            {
                //https://haveibeenpwned.com/api/v2/breachedaccount/test@example.com?truncateResponse=true
                args.Add("truncateResponse", fullResponse.ToString());
            }

            if (!string.IsNullOrEmpty(domain))
            {
                //https://haveibeenpwned.com/api/v2/breachedaccount/test@example.com?domain=adobe.com
                args.Add("domain", domain);
            }

            if (unVerified || !fullResponse || !string.IsNullOrEmpty(domain))
            {
                url = QueryHelpers.AddQueryString(url, args);
            }
            return GetRequestAsync<List<Breach>>(url, token);
        }

        ///<inheritdoc/>
        public Task<List<Breach>> GetAllBreachesAsync(string domain = "",
            CancellationToken token = default)
        {
            var args = new Dictionary<string, string>();

            //https://haveibeenpwned.com/api/v2/breaches
            var url = $"{_options.ServiceApiUrl}/{_getAllBreachesUri}";

            if (!string.IsNullOrEmpty(domain))
            {
                //https://haveibeenpwned.com/api/v2/breaches?domain=adobe.com
                args.Add("domain", WebUtility.UrlEncode(domain));
                url = QueryHelpers.AddQueryString(url, args);
            }

            return GetRequestAsync<List<Breach>>(url, token);
        }

        ///<inheritdoc/>
        public Task<List<string>> GetAllDataClassesAsync(CancellationToken token = default)
        {
            //https://haveibeenpwned.com/api/v2/dataclasses
            var url = $"{_options.ServiceApiUrl}/{_getAllDataClasses}";
            return GetRequestAsync<List<string>>(url, token);
        }

        ///<inheritdoc/>
        public Task<List<PastAccount>> GetPastByAccountAsync(string emailAccount,
            CancellationToken token = default)
        {
            //https://haveibeenpwned.com/api/v2/pasteaccount/test@example.com
            var url = $"{_options.ServiceApiUrl}/{_getPastAccount}/{WebUtility.UrlEncode(emailAccount)}";
            return GetRequestAsync<List<PastAccount>>(url, token);
        }

        private async Task<T> GetRequestAsync<T>(string url,
            CancellationToken token=default)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            var response = await Client.SendAsync(request, token).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(result);
        }
    }
}

