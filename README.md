# .NET Core 2.1 implementation of Haveibeenpwned.com RESTFul Api
[![Build status](https://ci.appveyor.com/api/projects/status/nn5g8qqeoyq4l46f?svg=true)](https://ci.appveyor.com/project/kdcllc/pwned-aspnetcore)

[Haveibeenpwned.com](https://haveibeenpwned.com/) provides a means to check if user's private information has been leaked or compromised. 
Pwned.AspNetCore .NET Core implementation of the restful api allows integration of this service within Asp.Net Core application.

This library can be used to extend the Asp.Net Core Identity library to support with `password` and `email` check to see if users' private information has been compromised by recent known data breach.

Users can enter an email address, and see a list of all known data breaches with records tied to that email address. In addition custom details queries can be made to retrieve information of the each data breach, such as the backstory of the breach and what specific types of data were included in it.

## Repo contains

This repo comes with two projects

1. Pwned.AspNetCore - library codebase around the restful api.

2. Pwned.Web - Asp.Net Core RazorPage sample application with Identity extension to support Pwned Password validation.

## Usage

1. Install

    Package Manager
    ```cmd
         Install-Package Pwned.AspNetCore
    ```
    .NET CLI
    ```cmd
        dotnet add package Pwned.AspNetCore
    ```

2. Add services configuration in Startup.cs

    ```c#
        services.AddPwned();
    ```
3. Add custom options configuration in `appsetting.json`
    ```json
     "Pwned": {
        "UserAgent": "Pwned.Web-Agent",
        "ServiceApiUrl": "https://haveibeenpwned.com/api/",
        "ServiceApiVersion": "2",
        "PasswordsApiUrl": "https://api.pwnedpasswords.com"
      }
    ```
4. Create Password Validator for Asp.Net Core Identity
    ```c#
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
    ```
