namespace Pwned.AspNetCore
{
    /// <summary>
    /// Options for <see cref="PwnedBreachService"/> and <see cref="PwnedPasswordService"/>
    /// </summary>
    /// <example>
    ///"Pwned": {
    ///    "UserAgent": "",
    ///    "ServiceApiUrl": "https://haveibeenpwned.com/api/",
    ///    "ServiceApiVersion": "2",
    ///    "PasswordsApiUrl": "https://api.pwnedpasswords.com"
    ///   }
    /// </example>
    public class PwnedOptions
    {
        /// <summary>
        /// User Agent for the application.
        /// </summary>
        public string UserAgent { get; set; }

        /// <summary>
        /// https://haveibeenpwned.com/api/v2/{service}/{parameter}
        /// </summary>
        public string ServiceApiUrl { get; set; } = @"https://haveibeenpwned.com/api/";

        /// <summary>
        /// Service Api Version for Breach and Paste.
        /// </summary>
        public string ServiceApiVersion { get; set; } = "2";

        /// <summary>
        /// https://haveibeenpwned.com/API/v2#PwnedPasswords
        /// </summary>
        public string PasswordsApiUrl { get; set; } = @"https://api.pwnedpasswords.com/";
    }
}
