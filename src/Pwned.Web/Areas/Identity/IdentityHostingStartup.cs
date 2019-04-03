using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pwned.AspNetCore;
using Pwned.Web.Areas.Identity.Data;
using Pwned.Web.Data;

[assembly: HostingStartup(typeof(Pwned.Web.Areas.Identity.IdentityHostingStartup))]
namespace Pwned.Web.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
                services.AddDbContext<PwnedWebContext>(options =>
                    options.UseSqlite(
                        context.Configuration.GetConnectionString("PwnedWebContextConnection")));


                services.AddDefaultIdentity<PwnedWebUser>()
                    .AddEntityFrameworkStores<PwnedWebContext>()
                    .AddPwnedPasswordValidator<PwnedWebUser>(context.Configuration);
                    //.AddPasswordValidator<PwnedPasswordValidator<PwnedWebUser>>();
            });
        }
    }
}
