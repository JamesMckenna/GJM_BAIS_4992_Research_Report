using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IdentityModel.Tokens.Jwt;


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

                //Don't include all User claims in id token,
                //a client needs to make a seperate request to UserInfo Endpoint to get all a User's claims
                options.GetClaimsFromUserInfoEndpoint = false;
                options.SaveTokens = true;
               
                options.ResponseType = "code";
                options.Scope.Clear();
                //Scope the client is requesting in the accees token
                options.Scope.Add("AnAPI");
                options.Scope.Add("offline_access");
                options.Scope.Add("openid");
                options.Scope.Add("invoiceRead");
                options.Scope.Add("invoiceManage");
                options.Scope.Add("identityManagementAdmin");
                options.Scope.Add("identityManagement");
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
