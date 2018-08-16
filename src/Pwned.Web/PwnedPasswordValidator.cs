using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Pwned.AspNetCore;

namespace Pwned.Web
{
    public class PwnedPasswordValidator<TUser> : IPasswordValidator<TUser>
        where TUser : IdentityUser
    {
        private readonly PwnedPasswordService _passwordService;

        public PwnedPasswordValidator(PwnedPasswordService passwordService)
        {
            _passwordService = passwordService;
        }
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
                }));
            }
            return await Task.FromResult(IdentityResult.Success);
        }
    }
}
