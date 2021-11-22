using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IdentityModel.Tokens.Jwt;



using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel;
using IdentityModel;


namespace MvcClient
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            //JwtSecurityTokenHandler.DefaultMapInboundClaims = true;
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            services.AddAccessTokenManagement();

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = "Cookies";
                options.DefaultChallengeScheme = "oidc";
            })
            .AddCookie("Cookies", options =>
            {
                options.Cookie.Name = "mvcClient";
                options.Events.OnSigningOut = async e =>
                {
                    await e.HttpContext.RevokeUserRefreshTokenAsync();
                };

            })
            .AddOpenIdConnect("oidc", options =>
            {
                options.Authority = "https://localhost:5001";

                options.ClientId = "mvcClient";
                options.ClientSecret = "aDifferentSecret";

                options.GetClaimsFromUserInfoEndpoint = false;
                options.SaveTokens = true;
               
                options.ResponseType = "code";
                options.Scope.Clear();
                options.Scope.Add("AnAPI");
                options.Scope.Add("offline_access");
                options.Scope.Add("openid");
                //options.Scope.Add("profile");
                options.Scope.Add("invoiceRead");
                options.Scope.Add("invoiceManage");
                //options.Scope.Add("customClaim");
                //options.Scope.Add("address");
                options.Scope.Add("identityManagementAdmin");
                options.Scope.Add("identityManagement");

                //options.ClaimActions.MapUniqueJsonKey("customClaim", "customClaim");
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
