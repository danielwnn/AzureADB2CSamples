using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.AzureADB2C.UI;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AzureB2CApiApp
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public IWebHostEnvironment _env;

        public static string ScopeRead;
        public static string ScopeWrite;

        public Startup(IWebHostEnvironment env)
        {
            _env = env;

            var builder = new ConfigurationBuilder()
                            .SetBasePath(env.ContentRootPath)
                            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                            .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see https://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets<Startup>();
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();

            ScopeRead = Configuration["AzureAdB2C:ScopeRead"];
            ScopeWrite = Configuration["AzureAdB2C:ScopeWrite"];
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var audience = Configuration["AzureAdB2C:ClientId"];
            var authority = Configuration["AzureAdB2C:Authority"];
            var metadataAddress = Configuration["AzureAdB2C:MetadataAddress"];

            Console.WriteLine("Audience -> " + audience);
            Console.WriteLine("authority -> " + authority);
            Console.WriteLine("MetadataAddress -> " + metadataAddress);

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                    builder => {
                        builder // .WithOrigins("https://localhost:5001", "http://localhost:5000")
                            .AllowAnyOrigin() 
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                    });
            });

            services.AddAuthentication(AzureADB2CDefaults.BearerAuthenticationScheme)
                .AddAzureADB2CBearer(options => Configuration.Bind("AzureAdB2C", options))
                .AddCookie(options => {
                    options.Cookie.Name = ".WebApi.Cookie";
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SecurePolicy = _env.IsDevelopment() ? CookieSecurePolicy.None : CookieSecurePolicy.Always;
                    options.Cookie.SameSite = SameSiteMode.Lax;
                });

            services.Configure<JwtBearerOptions>(
                AzureADB2CDefaults.JwtBearerAuthenticationScheme,
                options =>
                {
                    options.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = ctx => 
                        {
                            string objectId = ctx.Principal.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;
                            Console.WriteLine("User Object ID -> " + objectId);

                            // call graph API to get group membership and then map to roles
                            // fake the roles below
                            var claims = new List<Claim> { 
                                new Claim(ClaimTypes.Role, "Admins"), 
                                new Claim(ClaimTypes.Role, "Users") 
                            };
                            ctx.Principal.AddIdentity(new ClaimsIdentity(claims));

                            // add the access_token claim, by default Identity framework does expose it as claim
                            var accessToken = ctx.SecurityToken as JwtSecurityToken;
                            if (accessToken != null)
                            {
                                ClaimsIdentity identity = ctx.Principal.Identity as ClaimsIdentity;
                                identity.AddClaim(new Claim("access_token", accessToken.RawData));
                            }

                            return Task.CompletedTask;
                        }
                    };
                });

            /*
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
              .AddAuthentication(options =>
              {
                  options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                  options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                  options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
              })
              .AddJwtBearer(jwtOptions =>
              {
                  // jwtOptions.MetadataAddress = metadataAddress;
                  jwtOptions.Authority = authority;
                  jwtOptions.Audience = audience;

                  jwtOptions.Events = JwtBearerMiddlewareDiagnostics.Subscribe(jwtOptions.Events);

                  //jwtOptions.RequireHttpsMetadata = false;
                  //jwtOptions.SaveToken = true;
                  //jwtOptions.TokenValidationParameters = new TokenValidationParameters()
                  //{
                  //    ValidateAudience = false,
                  //    ValidateLifetime = false,
                  //    ValidateIssuerSigningKey = false
                  //};
              });
              */

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors();

            app.UseCookiePolicy();

            app.UseAuthentication();
            
            app.UseAuthorization();

            app.Use(async (httpContext, next) =>
            {
                //var principal = httpContext.User as ClaimsPrincipal;
                //var accessToken = principal?.Claims.FirstOrDefault(c => c.Type == "access_token");

                //if (accessToken != null)
                //{
                //    logger.LogInformation("Access_token -> " + accessToken.Value);
                //}

                // Get the encrypted cookie value
                //var cookieName = ".WebApi.Cookie";
                //var opt = httpContext.RequestServices.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>();
                //var cookie = opt.CurrentValue.CookieManager.GetRequestCookie(httpContext, cookieName);

                //// Decrypt if found
                //if (!string.IsNullOrEmpty(cookie))
                //{
                //    var dataProtector = opt.CurrentValue.DataProtectionProvider
                //        .CreateProtector("Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationMiddleware", cookieName, "v2");

                //    var ticketDataFormat = new TicketDataFormat(dataProtector);
                //    var ticket = ticketDataFormat.Unprotect(cookie);

                //    foreach (Claim c in ticket.Principal.Claims)
                //    {
                //        logger.LogInformation($"Claim in Cookie: {c.Type} -> {c.Value}");
                //    }
                //}

                await next();
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
