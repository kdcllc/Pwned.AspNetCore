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
        private readonly IPwnedPasswordService _passwordService;

        /// <summary>
        /// Constructor for <see cref="IPwnedPasswordService"/>.
        /// </summary>
        /// <param name="passwordService"></param>
        public PwnedPasswordValidator(IPwnedPasswordService passwordService)
        {
            _passwordService = passwordService;
        }

        ///<inheritdoc/>
        public async Task<IdentityResult> ValidateAsync(
            UserManager<TUser> manager,
            TUser user,
            string password)
        {
            var (pwned, count) = await _passwordService.IsPasswordPwnedAsync(password);

            if (pwned)
            {
                return await Task.FromResult(IdentityResult.Failed(new IdentityError
                {
                    Code = "PwnedPassword",
                    Description = $"The password you chose has appeared in a data breach {count} times. It is recommended that you change your password immediately"
                })).ConfigureAwait(false);
            }
            return await Task.FromResult(IdentityResult.Success).ConfigureAwait(false);
        }
    }
}
