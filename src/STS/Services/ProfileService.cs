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

namespace IS4Login.Services
{
    public class IS4LoginProfileService : IProfileService
    {
        private readonly IUserClaimsPrincipalFactory<ApplicationUser> _claimsFactory;
        private readonly UserManager<ApplicationUser> _userManager;

        public IS4LoginProfileService(UserManager<ApplicationUser> userManager, IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory)
        {
            _userManager = userManager;
            _claimsFactory = claimsFactory;
        }

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var sub = context.Subject.GetSubjectId();
            var user = await _userManager.FindByIdAsync(sub);
            var principal = await _claimsFactory.CreateAsync(user);

            var claims = principal.Claims.ToList();

            //https://damienbod.com/2016/11/18/extending-identity-in-identityserver4-to-manage-users-in-asp-net-core/
            //above link shows how to add Roles to claims, link is old but gives good guidance 

            claims = claims.Where(claim => context.RequestedClaimTypes.Contains(claim.Type)).ToList();

            claims.Add(new Claim(JwtClaimTypes.Name, user.UserName));

            claims.Add(new Claim("AppCustomClaim", "CustomClaim"));
            claims.Add(new Claim(IdentityServerConstants.StandardScopes.Email, user.Email));
            claims.Add(new Claim("Admin", "Admin"));
            claims.Add(new Claim("location", "somewhere"));

            context.IssuedClaims = claims;
        }

        public async Task IsActiveAsync(IsActiveContext context)
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
