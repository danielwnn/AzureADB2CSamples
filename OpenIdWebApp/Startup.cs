using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace OidcWeb
{
    public class Startup
    {
        private IConfiguration Configuration { get; }
		
        public Startup(IConfiguration configuration)
        {
            this.Configuration = new ConfigurationBuilder()
                .AddConfiguration(configuration)
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.local.json", true)
                .Build();
        }

        // Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddAuthentication(options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                    options.RequireAuthenticatedSignIn = true;
                })
                .AddOpenIdConnect(options =>
                {
                    this.Configuration.Bind("oidc", options);

                    options.TokenValidationParameters.NameClaimType = "name";
                    options.TokenValidationParameters.RoleClaimType = "role";

                    // Here you can set other options if you want to hard-code something instead of having it configurable.

                    options.Events.OnTicketReceived = async (context) =>
                    {
                        var user = context.Principal;
                        var identity = user.Identity as ClaimsIdentity;

                        // Here you can connect to other systems and augment the claims of the logged on user

                        await Task.Yield();
                    };
                })
                .AddCookie();

            services.AddControllers();
            services.AddRazorPages();
        }

        // Use this method to configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
            });
        }
    }
}
