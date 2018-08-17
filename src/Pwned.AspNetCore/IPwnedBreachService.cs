using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Pwned.AspNetCore.Models;

namespace Pwned.AspNetCore
{
    /// <summary>
    /// A client for communicating with HaveIBeenPwned.com API
    /// </summary>
    public interface IPwnedBreachService
    {
        /// <summary>
        /// <see cref="HttpClient"/> instance instantiated by DI.
        /// </summary>
        HttpClient Client { get; }

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
        Task<List<Breach>> GetBreachesByAccountAsync(string emailAccount, bool unVerified = false, bool fullResponse = true, string domain = "", CancellationToken token = default);

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
        Task<List<Breach>> GetAllBreachesAsync(string domain = "", CancellationToken token = default);

        /// <summary>
        /// A "data class" is an attribute of a record compromised in a breach. 
        /// For example, many breaches expose data classes such as "Email addresses" and "Passwords". 
        /// The values returned by this service are ordered alphabetically in a string array 
        /// and will expand over time as new breaches expose previously unseen classes of data.
        /// </summary>
        /// <param name="token"><see cref="CancellationToken"/></param>
        /// <returns></returns>
        Task<List<string>> GetAllDataClassesAsync(CancellationToken token = default);

        /// <summary>
        /// Unlike searching for breaches, usernames that are not email addresses cannot be searched for.
        /// </summary>
        /// <param name="emailAccount"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<List<PastAccount>> GetPastByAccountAsync(string emailAccount, CancellationToken token = default);
    }
}
