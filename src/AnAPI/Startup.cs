using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace AnAPI
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
            services.AddControllers();

            services.AddCors(options =>
            {
                //this defines a CORS policy called "default"
                options.AddPolicy("default", policy =>
                {
                    policy.WithOrigins("https://localhost:5003")
                    //Set up a proper Content Security policy and DON'T allow any header or method
                    //See STS project for example, too much for report's length contraints
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });


            //Only allow ApiScopes and APIResources configured with STS
            //Block any request that don't have these scopes in the access token passed from client
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AnAPIScope", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("AnAPI");
                    //Can also lock down API endpoints by requiring specific cliams in access token
                    //policy.RequireClaim("requiredClaim1", "requiredClaim2");
                });
            });


            //Look for token in request's Authorization Header as a Bearer Token
            services.AddAuthentication("Bearer").AddJwtBearer("Bearer", options =>
            {
                //The STS who made the tokens and to verify token against
                options.Authority = "https://localhost:5001";
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = false
                };
            });


            //Further details out of project's lenght contraint
            //Example of an Authorization Handler for Reference Tokens,
            //causes multiple trips to and from STS introspection endpoint
            //But more secure that self-contained access tokens 
            //.AddOAuth2Introspection("introspection", options =>
            //{
            //    options.Authority = STS URL;
            //    //Allow a specifc client to call this API
            //    options.ClientId = name of client registered with STS;
            //    options.ClientSecret = that client's secret;
            //})
            //.AddIdentityServerAuthentication("IdentityServerAccessToken", options =>
            //{
            //    options.Authority = STS URL;
            //    options.ApiName = Name of this API, must match name in APIResource registered STS Config.cs;
            //    options.ApiSecret = Secret of this API, must match secret in APIResource registered STS Config.cs;

            //    //Using Reference Tokens - lessons calls to IS4 to validate tokens
            //    options.EnableCaching = true; //REQUIRES: caching strategy - in memory, redis ect...;
            //    options.CacheDuration = TimeSpan.FromMinutes(2); 
            //    options.SupportedTokens = SupportedTokens.Both; This handler will support Jwts and Reference Tokens
            //});
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseStaticFiles();
            app.UseHttpsRedirection();

            app.UseRouting();

            //Register needed middleware 
            app.UseCors("default");
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
