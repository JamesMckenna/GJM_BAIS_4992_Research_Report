using IdentityModel;
using IdentityServer4;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using STS.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace STS.Services
{
    public class ProfileService : DefaultProfileService
    {
        private readonly IUserClaimsPrincipalFactory<ApplicationUser> _claimsFactory;
        private readonly UserManager<ApplicationUser> _userManager;
        

        public ProfileService(UserManager<ApplicationUser> userManager, IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory, ILogger<DefaultProfileService> logger) : base(logger)
        {
            _userManager = userManager;
            _claimsFactory = claimsFactory;
        }

        public override async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var sub = context.Subject.GetSubjectId();
            var user = await _userManager.FindByIdAsync(sub);
            var principal = await _claimsFactory.CreateAsync(user);

            var claims = principal.Claims.ToList();

            claims = claims.Where(claim => context.RequestedClaimTypes.Contains(claim.Type)).ToList();

            if (context.Caller == "ClaimsProviderAccessToken")
            {
                claims.Add(new Claim(JwtClaimTypes.Name, user.UserName));
                claims.Add(new Claim(IdentityServerConstants.StandardScopes.OpenId, sub));
                claims.Add(new Claim(JwtClaimTypes.Scope, "AnAPI"));
                claims.Add(new Claim(JwtClaimTypes.Scope, "read"));
                claims.Add(new Claim(JwtClaimTypes.Scope, "write"));
                if (!String.IsNullOrWhiteSpace(principal.Claims.Where(claim => claim.Type == "identityManagement").FirstOrDefault()?.Value))
                {
                    claims.Add(new Claim("identityManagement", principal.Claims.Where(claim => claim.Type == "identityManagement").FirstOrDefault()?.Value));
                }
                if (!String.IsNullOrWhiteSpace(principal.Claims.Where(claim => claim.Type == "identityManagement").FirstOrDefault()?.Value))
                {
                    claims.Add(new Claim("identityManagementAdmin", principal.Claims.Where(claim => claim.Type == "identityManagementAdmin").FirstOrDefault()?.Value));
                }
            }

            if (context.Caller == "ClaimsProviderIdentityToken")
            {
                claims.Add(new Claim(JwtClaimTypes.Name, user.UserName));
                claims.Add(new Claim(IdentityServerConstants.StandardScopes.OpenId, sub));
                claims.Add(new Claim(IdentityServerConstants.StandardScopes.Email, user.Email));
            }

            if (context.Caller == "UserInfoEndpoint")
            {
                claims.Add(new Claim(JwtClaimTypes.Name, user.UserName));
                claims.Add(new Claim(IdentityServerConstants.StandardScopes.OpenId, sub));
                claims.Add(new Claim(IdentityServerConstants.StandardScopes.Profile, user.UserName));
                claims.Add(new Claim(IdentityServerConstants.StandardScopes.Email, user.Email));
                //Some users may have claims other may not, so check before trying to pass a claim with value of null
                if (!String.IsNullOrWhiteSpace(principal.Claims.Where(claim => claim.Type == "address").FirstOrDefault()?.Value))
                {
                    claims.Add(new Claim("address", principal.Claims.Where(claim => claim.Type == "address").FirstOrDefault()?.Value));
                }
                if (!String.IsNullOrWhiteSpace(principal.Claims.Where(claim => claim.Type == "customClaim").FirstOrDefault()?.Value))
                {
                    claims.Add(new Claim("customClaim", principal.Claims.Where(claim => claim.Type == "customClaim").FirstOrDefault()?.Value));
                }
                if (!String.IsNullOrWhiteSpace(principal.Claims.Where(claim => claim.Type == "website").FirstOrDefault()?.Value))
                {
                    claims.Add(new Claim("website", principal.Claims.Where(claim => claim.Type == "website").FirstOrDefault()?.Value));
                }
            }

            context.IssuedClaims = claims;
        }

        public override async Task IsActiveAsync(IsActiveContext context)
        {
            var sub = context.Subject.GetSubjectId();
            var user = await _userManager.FindByIdAsync(sub);
            var active = (user != null && (!user.LockoutEnabled || user.LockoutEnd == null)) ||
            (user != null && user.LockoutEnabled && user.LockoutEnd != null &&
            DateTime.UtcNow > user.LockoutEnd);

            context.IsActive = active;
        }
    }
}
