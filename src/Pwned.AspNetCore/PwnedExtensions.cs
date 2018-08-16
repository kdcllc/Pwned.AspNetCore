using System;
using System.Net;
using System.Net.Http;
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
        /// An extension method that adds support for:
        /// 1. <see cref="PwnedBreachService"/>
        /// 2. <see cref="PwnedPasswordService"/>
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddPwned(this IServiceCollection services)
        {
            var provider = services.BuildServiceProvider();
            var config = provider.GetRequiredService<IConfiguration>();

            services.Configure<PwnedOptions>(config.GetSection("Pwned"));

            var timeout = Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(10));

            services.AddHttpClient("breach")
                .AddPolicyHandler(timeout)
                .AddPolicyHandler(ExponentialWaitAndRetry(2))
                .AddTypedClient<PwnedBreachService>();

            services.AddHttpClient("password")
                .AddTransientHttpErrorPolicy(p => p.RetryAsync(2))
                .AddTypedClient<PwnedPasswordService>();

            return services;
        }

        //https://github.com/App-vNext/Polly/issues/414#issuecomment-371932576
        private static IAsyncPolicy<HttpResponseMessage> ExponentialWaitAndRetry(int retry)
        {
            return Policy.Handle<HttpRequestException>().OrResult<HttpResponseMessage>
                (r =>  r.StatusCode == (HttpStatusCode)429) // RetryAfter
              .WaitAndRetryAsync(retry, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }

    }
}
