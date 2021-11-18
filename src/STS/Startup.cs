// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4;
using STS.Data;
using STS.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using IdentityServer4.Services;
using IdentityServer4.AspNetIdentity;
using Microsoft.AspNetCore.Http;
using IdentityServer4.Configuration;
using System;
using Microsoft.AspNetCore.Authentication.Cookies;
using STS.Services.SecurityHeaders;

namespace STS
{
    public class Startup
    {
        public IWebHostEnvironment Environment { get; }
        public IConfiguration Configuration { get; }

        public Startup(IWebHostEnvironment environment, IConfiguration configuration)
        {
            Environment = environment;
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(Configuration.GetConnectionString("DefaultConnection")));

            services.AddCors(options =>
            {
                options.AddPolicy("STSCORS", policy =>
                {
                    policy.WithOrigins("https://localhost:5005", "https://localhost:6001", "https://localhost:5003", "https://localhost:5002");
                        //.AllowAnyHeader()
                        //.AllowAnyMethod()
                        //.AllowAnyOrigin();
                });
            });

            services.AddIdentity<ApplicationUser, IdentityRole>( options => {
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 1;

                options.SignIn.RequireConfirmedEmail = true;
                options.SignIn.RequireConfirmedPhoneNumber = false;

                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._+";
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            var builder = services.AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;

                // see https://identityserver4.readthedocs.io/en/latest/topics/resources.html
                options.EmitStaticAudienceClaim = true;

                options.UserInteraction = new UserInteractionOptions
                {
                    LogoutUrl = "/Account/Logout",
                    LoginUrl = "/Account/Login",
                    LoginReturnUrlParameter = "returnUrl"
                };

                options.Csp.Level = IdentityServer4.Models.CspLevel.Two;
              
                //Session Cookie
                options.Authentication.CheckSessionCookieName = "STSSessionCookie";
                options.Authentication.CookieLifetime = TimeSpan.FromSeconds(3600);
                options.Authentication.CookieSlidingExpiration = true;

                options.Authentication.CookieSameSiteMode = SameSiteMode.Strict;
                options.Authentication.CheckSessionCookieSameSiteMode = SameSiteMode.Strict;

                options.Authentication.RequireCspFrameSrcForSignout = false;
                options.MutualTls.Enabled = true;
                options.Cors.CorsPolicyName = "STSCORS";
            })
            .AddAspNetIdentity<ApplicationUser>()
            .AddProfileService<ProfileService<ApplicationUser>>()
            // this adds the config data from DB (clients, resources, CORS)
            .AddConfigurationStore(options =>
            {
                options.ConfigureDbContext = builder => builder.UseSqlite(Configuration.GetConnectionString("STSConfigurationConnection"));
            })
            // this adds the operational data from DB (codes, tokens, consents)
            .AddOperationalStore(options =>
            {
                options.ConfigureDbContext = builder => builder.UseSqlite(Configuration.GetConnectionString("STSOperationalConnection"));

                // this enables automatic token cleanup. this is optional.
                options.EnableTokenCleanup = true;
            });

            services.AddAntiforgery(options =>
            {
                options.Cookie.Name = "STSAnitForgeryCookie";
                options.SuppressXFrameOptionsHeader = true;
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            });


            // not recommended for production - you need to store your key material somewhere secure
            builder.AddDeveloperSigningCredential();

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie("Cookies")
            //Example of External IdP configuration
            .AddGoogle(options =>
            {
                options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                    
                // register your IdentityServer with Google at https://console.developers.google.com
                // enable the Google+ API
                // set the redirect URI to https://localhost:5001/signin-google
                options.ClientId = "copy client ID from Google here";
                options.ClientSecret = "copy client secret from Google here";
            });

            services.ConfigureApplicationCookie(options => {
                options.Cookie.Name = "STSCookie";

                options.Cookie.HttpOnly = true;

                options.Cookie.Path = "/";
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                //options.Cookie.HttpOnly = HttpOnlyPolicy.Always;
                options.Cookie.IsEssential = true;
                options.Cookie.SameSite = SameSiteMode.Strict;
                //Configures the ticket lifetime inside the cookie; not the cookie lifetime
                //This is separate from the value of , which specifies how long the browser will keep the cookie,
                //which should be controlled and set in STS Options
                options.ExpireTimeSpan = TimeSpan.FromSeconds(3600);
                //This is for session lifetimes....not token lifetime
                options.SlidingExpiration = true;
            });
            
            services.AddTransient<IProfileService, ProfileService<ApplicationUser>>();
        }

        public void Configure(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }

            app.UseCors("STSCORS");


            app.UseSecurityHeadersMiddleware(new SecurityHeadersBuilder()
              .AddDefaultSecurePolicy()
              .AddCustomHeader("Access-Control-Allow-Origin", "*")
              .AddCustomHeader("Content-Security-Policy", $"frame-ancestors 'self' https://localhost:5003;")
            );

            app.UseStaticFiles();

            app.UseRouting();
            
            app.UseIdentityServer();
            app.UseCookiePolicy();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}