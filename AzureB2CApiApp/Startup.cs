using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.AzureADB2C.UI;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AzureB2CApiApp
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public static string ScopeRead;
        public static string ScopeWrite;

        public Startup(IWebHostEnvironment env)
        {
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
                        builder.WithOrigins("https://localhost:5001", "http://localhost:5000")
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                    });
            });

            services.AddAuthentication(AzureADB2CDefaults.BearerAuthenticationScheme)
                .AddAzureADB2CBearer(options => Configuration.Bind("AzureAdB2C", options));

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
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors();

            app.UseAuthentication();
            
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
