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
            //Clear Microsoft default Claim mapping
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            //Adds user and client access token management with IdentityModel library
            services.AddAccessTokenManagement();
            
            services.AddAuthentication(options =>
            {
                //Token are in cookies
                options.DefaultScheme = "Cookies";
                //Use OpenID Connect protocol
                options.DefaultChallengeScheme = "oidc";
            })
            //Create this application's cookie to hold token 
            .AddCookie("Cookies", options =>
            {
                options.Cookie.Name = "mvcClient";
                options.Events.OnSigningOut = async e =>
                {
                    await e.HttpContext.RevokeUserRefreshTokenAsync();
                };

            })

            //Authorization Handler, use OpenID Connect handler
            .AddOpenIdConnect("oidc", options =>
            {
                options.Authority = "https://localhost:5001";

                options.ClientId = "mvcClient";
                options.ClientSecret = "aDifferentSecret";

                //Indicates that the authentication session lifetime (e.g. cookies)
                //should match that of the authentication token.
                //If the token does not provide lifetime information then normal
                //session lifetimes will be used. This is disabled by default.
                options.UseTokenLifetime = true;

                //Don't include all User claims in id token,
                //a client needs to make a seperate request to
                //UserInfo Endpoint to get all a User's claims
                //True will put all Identity Resources registered for this client in the tokens
                //False only puts some Identity Resources in tokens
                //Get the rest from endpoint - see ProfileService covered in report
                options.GetClaimsFromUserInfoEndpoint = false;
                options.SaveTokens = true;


                //OAuth 2.0 / OpenID Connect GrantType registered with STS Config.cs
                options.ResponseType = "code";
                //Clean slate
                options.Scope.Clear();
                //Scopes the client is requesting in the access token must match STS Config.cs
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
            //Add Autnentication to request/response pipeline (middleware)
            app.UseAuthentication();
            //Add Authentication to request/response pipeline (middleware)
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
