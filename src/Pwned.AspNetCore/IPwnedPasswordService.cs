﻿using System.Net.Http;
using System.Threading;

namespace Pwned.AspNetCore
{
    /// <summary>
    /// Haveibeenpwned.com password service.
    /// </summary>
    public interface IPwnedPasswordService
    {
        /// <summary>
        /// Instance of <see cref="HttpClient"/> that was created by DI.
        /// </summary>
        HttpClient Client { get; }

        /// <summary>
        /// Returns true if password was compromised and number of times it has been compromised.
        /// </summary>
        /// <param name="password">Does not store password anywhere simply hashes it and sends to the Resful Api.</param>
        /// <param name="token"><see cref="CancellationToken"/></param>
        /// <returns></returns>
        System.Threading.Tasks.Task<(bool pwned, long count)> IsPasswordPwned(string password, CancellationToken token = default);
    }
}
