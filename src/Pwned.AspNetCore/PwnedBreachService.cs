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
    public class PwnedBreachService
    {
        private readonly PwnedOptions _options;
        private readonly string _getBreachesByAccountUri = "breachedaccount";
        private readonly string _getAllBreachesUri = "breaches";
        private readonly string _getAllDataClasses = "dataclasses";
        private readonly string _getPastAccount = "pasteaccount";

        /// <summary>
        /// <see cref="HttpClient"/> instance instantiated by DI.
        /// </summary>
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

        /// <summary>
        /// Returns full or short <see cref="Breach"/> breach information about compromised email account.
        /// </summary>
        /// <param name="emailAccount">An email address of the compromised user.</param>
        /// <param name="unVerified">Default is false, unverified breaches.
        ///  Filters breaches that have been flagged as "unverified". 
        ///  By default, only verified breaches are returned web performing a search.
        /// </param>
        /// <param name="fullResponse">Default is true and returns a full set of data about the breach.</param>
        /// <param name="domain">Filters the result set to only breaches against the domain specified. 
        /// It is possible that one site (and consequently domain), is compromised on multiple occasions.</param>
        /// <param name="token"><see cref="CancellationToken"/></param>
        /// <returns></returns>
        public async Task<List<Breach>> GetBreachesByAccountAsync(string emailAccount,
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
            return await GetRequestAsync<List<Breach>>(url, token).ConfigureAwait(false);
        }

        /// <summary>
        /// A "breach" is an instance of a system having been compromised by an attacker and the data disclosed. 
        /// For example, Adobe was a breach, Gawker was a breach etc. 
        /// It is possible to return the details of each of breach in the system which currently stands at 300 breaches.
        /// </summary>
        /// <param name="domain">
        /// Filters the result set to only breaches against the domain specified. 
        /// It is possible that one site (and consequently domain), is compromised on multiple occasions.
        /// </param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<List<Breach>> GetAllBreachesAsync(string domain = "",
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

            return await GetRequestAsync<List<Breach>>(url, token).ConfigureAwait(false);
        }

        /// <summary>
        /// A "data class" is an attribute of a record compromised in a breach. 
        /// For example, many breaches expose data classes such as "Email addresses" and "Passwords". 
        /// The values returned by this service are ordered alphabetically in a string array 
        /// and will expand over time as new breaches expose previously unseen classes of data.
        /// </summary>
        /// <param name="token"><see cref="CancellationToken"/></param>
        /// <returns></returns>
        public async Task<List<string>> GetAllDataClasses(CancellationToken token = default)
        {
            //https://haveibeenpwned.com/api/v2/dataclasses
            var url = $"{_options.ServiceApiUrl}/{_getAllDataClasses}";
            return await GetRequestAsync<List<string>>(url, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Unlike searching for breaches, usernames that are not email addresses cannot be searched for.
        /// </summary>
        /// <param name="emailAccount"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<List<PastAccount>> GetPastByAccount(string emailAccount,
            CancellationToken token = default)
        {
            //https://haveibeenpwned.com/api/v2/pasteaccount/test@example.com
            var url = $"{_options.ServiceApiUrl}/{_getPastAccount}/{WebUtility.UrlEncode(emailAccount)}";
            return await GetRequestAsync<List<PastAccount>>(url, token).ConfigureAwait(false);
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

