# .NET Core 2.1 implementation of Haveibeenpwned.com RESTFul Api
[![Build status](https://ci.appveyor.com/api/projects/status/nn5g8qqeoyq4l46f?svg=true)](https://ci.appveyor.com/project/kdcllc/pwned-aspnetcore)

[Haveibeenpwned.com](https://haveibeenpwned.com/) provides a means to check if user's private information has been leaked or compromised. 
Pwned.AspNetCore .NET Core implementation of the restful api allows integration of this service within Asp.Net Core application.

This library can be used to extend the Asp.Net Core Identity library to support with `password` and `email` check to see if users' private information has been compromised by recent known data breach.

Users can enter an email address, and see a list of all known data breaches with records tied to that email address. In addition custom details queries can be made to retrieve information of the each data breach, such as the backstory of the breach and what specific types of data were included in it.

## Repo contains

This repo comes with two projects

1. Pwned.AspNetCore - library codebase around the restful api.

    - [Get all Breaches](https://haveibeenpwned.com/api/v2/breaches)
    - [Get all breaches for an email account.](https://haveibeenpwned.com/api/v2/breachedaccount/test@example.com)
    - [Get all breaches for an email account - truncated filter.](https://haveibeenpwned.com/api/v2/breachedaccount/test@example.com?truncateResponse=true)
    - [Get all breaches for an email account - domain filter.](https://haveibeenpwned.com/api/v2/breachedaccount/test@example.com?domain=adobe.com)
    - [Get all breaches for an email account - include unverified filter.](https://haveibeenpwned.com/api/v2/breachedaccount/test@example.com?includeUnverified=true)
    - [Get all breached sites in the system.](https://haveibeenpwned.com/api/v2/breaches?domain=adobe.com)
    - [Get a single breached site.](https://haveibeenpwned.com/api/v2/breach/Adobe)
    - [Get all data classes in the system.](https://haveibeenpwned.com/api/v2/dataclasses)
    - [Get all all pastes for an email account.](https://haveibeenpwned.com/api/v2/pasteaccount/test@example.com)
    - [Pwned Passwords.](https://api.pwnedpasswords.com/range/21BD1)

2. Pwned.Web - Asp.Net Core RazorPage sample application with Identity extension to support Pwned Password validation.

## Getting Started

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
    Or configuration can be done thru HttpClientExension
    ```c#
        services.AddPwnedBreachHttpClient()
               .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(30)))
               .AddPolicyHandler(PwnedExtensions.ExponentialWaitAndRetry(2));

        services.AddPwnedPasswordHttpClient()
                .AddTransientHttpErrorPolicy(p => p.RetryAsync(3))
                .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(2)));
    ```
3. Add custom options configuration in `appsetting.json`
    ```json
     "Pwned": {
        "UserAgent": "Pwned.Web-Agent",
        "ServiceApiUrl": "https://haveibeenpwned.com/api/",
        "ServiceApiVersion": "2",
        "PasswordsApiUrl": "https://api.pwnedpasswords.com/"
      }
    ```
4. Add Custom Password Validator to the Asp.net Core Identity
    ```c#
     services.AddDefaultIdentity<PwnedWebUser>()
                    .AddEntityFrameworkStores<PwnedWebContext>()
                    .AddPwnedPasswordValidator<PwnedWebUser>();
    ```

## Additional Resources
- [HttpClientFactory in ASP.NET Core 2.1 ](https://www.stevejgordon.co.uk/introduction-to-httpclientfactory-aspnetcore)
