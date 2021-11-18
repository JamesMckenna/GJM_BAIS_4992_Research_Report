using IdentityManagement.Data;
using IdentityManagement.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using IdentityModel;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace IdentityManagement
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
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(Configuration.GetConnectionString("DefaultConnection")));

            services.AddDatabaseDeveloperPageExceptionFilter();

            services.AddIdentityCore<ApplicationUser>(options => {
                options.SignIn.RequireConfirmedAccount = true;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 1;

                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-@._+";
                options.User.RequireUniqueEmail = true;

            })
            .AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders().AddDefaultUI();

            services.AddCors(options =>
            {
                options.AddPolicy("default", policy =>
                {
                    policy.WithOrigins("https://localhost:5001", "https://localhost:5002", "https://localhost:5003").AllowAnyHeader().AllowAnyMethod();
                });
            });


            services.AddAuthentication(options =>
            {
                options.DefaultScheme = "Cookies";
                options.DefaultChallengeScheme = "oidc";
            })
            .AddCookie("Cookies")
            .AddOpenIdConnect("oidc", options =>
            {
                options.Authority = "https://localhost:5001";

                options.ClientId = "IdentityManagement";
                options.ClientSecret = "AnotherSecret";

                options.SignedOutCallbackPath = new PathString("/signout-callback-oidc");
                options.CallbackPath = "/signin-oidc";

                options.GetClaimsFromUserInfoEndpoint = true;
                options.SaveTokens = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = "name",     
                };

                options.ResponseType = "code";
                options.UsePkce = true;
                options.RequireHttpsMetadata = true;

                //Indicates that the authentication session lifetime (e.g. cookies) should match that of the authentication token.
                //If the token does not provide lifetime information then normal session lifetimes will be used. This is disabled by default.
                options.UseTokenLifetime = true;
                options.Events.OnTicketReceived = (context) =>//IF ticket is Identity Ticket (Authentication)
                {
                    context.Properties.IssuedUtc = DateTime.UtcNow;
                    //Part 1 of Session cookie lifetime. Part 2 is in cookie options
                    //setting of the ticket that is stored inside the cookie
                    //This ticket determines the validity of the users authentication session
                    context.Properties.ExpiresUtc = DateTime.UtcNow.AddSeconds(300);

                    context.Properties.IsPersistent = false;
                    context.Properties.AllowRefresh = true;
                    return Task.CompletedTask;
                };

                options.Events = new OpenIdConnectEvents
                {
                    OnRemoteFailure = (context) =>
                    {
                        context.Response.Redirect("/Account/AccessDenied");
                        context.HandleResponse();
                        return Task.CompletedTask;
                    },
                };

                options.Scope.Add("profile");
                options.Scope.Add("openid");
                options.Scope.Add("AnAPI");
                options.Scope.Add("offline_access");
                options.Scope.Add("Admin");
            });

            services.AddRazorPages();
            services.AddAuthorization();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }
    }
}
