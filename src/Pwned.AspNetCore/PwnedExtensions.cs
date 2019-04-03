using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Polly;
using Pwned.AspNetCore;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// An extension class for Pwned.AspNetCore
    /// </summary>
    public static class PwnedExtensions
    {
        /// <summary>
        /// Default Breach Name for <see cref="HttpClientBuilderExtensions"/>
        /// </summary>
        public const string DefaultBreachName = nameof(PwnedBreachService);

        /// <summary>
        /// Default configuration name.
        /// </summary>
        public const string Pwned = nameof(Pwned);

        /// <summary>
        /// Default Password Name for <see cref="HttpClientBuilderExtensions"/>
        /// </summary>
        public const string DefaultPasswordName = nameof(PwnedPasswordService);

        /// <summary>
        /// Adds <see cref="PwnedBreachService"/> and <see cref="PwnedPasswordService"/> and related services.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <param name="sectionName"></param>
        /// <returns></returns>
        public static IServiceCollection AddPwned(
            this IServiceCollection services,
            IConfiguration configuration,
            string sectionName = Pwned)
        {
            services.AddPwned(_ => configuration.GetSection(sectionName));
            return services;
        }

        public static IServiceCollection AddPwned(this IServiceCollection services)
        {
            services.AddPwned(_ => new PwnedOptions());

            return services;
        }

        /// <summary>
        /// Adds <see cref="PwnedBreachService"/> and <see cref="PwnedPasswordService"/> and related services.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IServiceCollection AddPwned(
            this IServiceCollection services,
            Action<PwnedOptions> options)
        {
            AddPwnedBreach(services, options);
            AddPwnedPassword(services, options);

            return services;
        }

        /// <summary>
        /// Adds <see cref="PwnedBreachService"/> and related services.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IServiceCollection AddPwnedBreach(
            this IServiceCollection services,
            Action<PwnedOptions> options)
        {
            services.Configure(options);

            services.AddHttpClient(DefaultBreachName)
               .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(30)))
               .AddPolicyHandler(ExponentialWaitAndRetry(2))
               .AddTypedClient<IPwnedBreachService,PwnedBreachService>();

            return services;
        }

        /// <summary>
        /// Adds <see cref="PwnedPasswordService"/> and related services.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddPwnedPassword(this IServiceCollection services)
        {
            return services.AddPwnedPassword(_ => new PwnedOptions());
        }

        /// <summary>
        /// Adds <see cref="PwnedPasswordService"/> and related services.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IServiceCollection AddPwnedPassword(
            this IServiceCollection services,
            Action<PwnedOptions> options)
        {
            services.Configure(options);

            // The pwnedpassword API achieves 99% percentile of <1s, so this should be sufficient
            services.AddHttpClient(DefaultPasswordName)
                .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(2)))
                .AddTransientHttpErrorPolicy(p => p.RetryAsync(3))
                .AddTypedClient<IPwnedPasswordService,PwnedPasswordService>();
            return services;
        }

        /// <summary>
        /// Adds the <see cref="IHttpClientFactory"/> and related services to the <see cref="IServiceCollection"/>
        /// and configures a binding between the <see cref="IPwnedPasswordService"/> and a named <see cref="HttpClient"/>
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <param name="name"></param>
        /// <param name="configureClient"></param>
        /// <param name="sectionName"></param>
        /// <returns></returns>
        public static IHttpClientBuilder AddPwnedPasswordHttpClient(
            this IServiceCollection services,
            IConfiguration configuration,
            string name,
            Action<HttpClient> configureClient,
            string sectionName = Pwned)
        {
            services.Configure<PwnedOptions>(name, configuration.GetSection(sectionName));

            return services.AddHttpClient<IPwnedPasswordService, PwnedPasswordService>(name, configureClient);
        }

        /// <summary>
        /// Adds the <see cref="IHttpClientFactory"/> and related services to the <see cref="IServiceCollection"/>
        /// and configures a binding between the <see cref="IPwnedPasswordService"/> and an <see cref="HttpClient"/>
        /// named <see cref="DefaultPasswordName"/> to use the public HaveIBeenPwned API
        /// at "https://api.pwnedpasswords.com"
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <param name="sectionName"></param>
        /// <returns></returns>
        public static IHttpClientBuilder AddPwnedPasswordHttpClient(
            this IServiceCollection services,
            IConfiguration configuration,
            string sectionName = Pwned)
        {
            return services.AddPwnedPasswordHttpClient(configuration, DefaultPasswordName, _ => { }, sectionName);
        }

        /// <summary>
        /// Adds the <see cref="IHttpClientFactory"/> and related services to the <see cref="IServiceCollection"/>
        /// and configures a binding between the <see cref="IPwnedBreachService"/> and an <see cref="HttpClient"/>
        /// named <see cref="DefaultBreachName"/> to use the public HaveIBeenPwned API
        /// at "https://pwnedpasswords.com"
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IHttpClientBuilder AddPwnedBreachHttpClient(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            return services.AddPwnedBreachHttpClient(configuration, DefaultBreachName, _ => { });
        }

        /// <summary>
        /// Adds the <see cref="IHttpClientFactory"/> and related services to the <see cref="IServiceCollection"/>
        /// and configures a binding between the <see cref="IPwnedBreachService"/> and an <see cref="HttpClient"/>
        /// named <see cref="DefaultBreachName"/> to use the public HaveIBeenPwned API
        /// at "https://pwnedpasswords.com"
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <param name="name"></param>
        /// <param name="configureClient"></param>
        /// <param name="sectionName"></param>
        /// <returns></returns>
        public static IHttpClientBuilder AddPwnedBreachHttpClient(
            this IServiceCollection services,
            IConfiguration configuration,
            string name,
            Action<HttpClient> configureClient,
            string sectionName = Pwned)
        {
            services.Configure<PwnedOptions>(name, configuration.GetSection(sectionName));

            return services.AddHttpClient<IPwnedBreachService, PwnedBreachService>(name, configureClient);
        }

        /// <summary>
        /// Adds a password validator that checks the password is not a pwned password using the Have I been pwned API
        /// See https://haveibeenpwned.com/API/v2#PwnedPasswords for details.
        /// </summary>
        /// <typeparam name="TUser"></typeparam>
        /// <param name="builder"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IdentityBuilder AddPwnedPasswordValidator<TUser>(
            this IdentityBuilder builder,
            IConfiguration configuration) where TUser : class
        {
            return builder.AddPwnedPasswordValidator<TUser>(configure: _ => configuration.GetSection("Pwned"));
        }

        /// <summary>
        /// Adds a password validator that checks the password is not a pwned password using the Have I been pwned API
        /// See https://haveibeenpwned.com/API/v2#PwnedPasswords for details.
        /// </summary>
        /// <typeparam name="TUser"></typeparam>
        /// <param name="builder"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static IdentityBuilder AddPwnedPasswordValidator<TUser>(
            this IdentityBuilder builder,
            Action<PwnedOptions> configure)
            where TUser : class
        {
            if (!builder.Services.Any(x => x.ServiceType == typeof(IPwnedPasswordService)))
            {
                builder.Services.AddPwnedPassword(configure);
            }
            return builder.AddPasswordValidator<PwnedPasswordValidator<TUser>>();
        }

        //https://github.com/App-vNext/Polly/issues/414#issuecomment-371932576
        public static IAsyncPolicy<HttpResponseMessage> ExponentialWaitAndRetry(int retry)
        {
            return Policy.Handle<HttpRequestException>().OrResult<HttpResponseMessage>
                (r =>  r.StatusCode == (HttpStatusCode)429) // RetryAfter
              .WaitAndRetryAsync(retry, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }
    }
}
