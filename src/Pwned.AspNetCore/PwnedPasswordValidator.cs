using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Pwned.AspNetCore
{
    /// <summary>
    /// An <see cref="IPasswordValidator{TUser}"/> for verifying a given password has not appeared in a data breach
    /// </summary>
    /// <typeparam name="TUser"></typeparam>
    public class PwnedPasswordValidator<TUser> : IPasswordValidator<TUser>
        where TUser : class
    {
        private readonly PwnedPasswordService _passwordService;

        /// <summary>
        /// Constructor for <see cref="PwnedPasswordService"/>.
        /// </summary>
        /// <param name="passwordService"></param>
        public PwnedPasswordValidator(PwnedPasswordService passwordService)
        {
            _passwordService = passwordService;
        }

        ///<inheritdoc/>
        public async Task<IdentityResult> ValidateAsync(UserManager<TUser> manager,
            TUser user, string password)
        {
            var (pwned, count) = await _passwordService.IsPasswordPwned(password);

            if (pwned)
            {
                return await Task.FromResult(IdentityResult.Failed(new IdentityError
                {
                    Code = "PwnedPassword",
                    Description = $"Your password has been compromised/pwned {count} times. Please use a different password."
                })).ConfigureAwait(false);
            }
            return await Task.FromResult(IdentityResult.Success).ConfigureAwait(false);
        }
    }
}
