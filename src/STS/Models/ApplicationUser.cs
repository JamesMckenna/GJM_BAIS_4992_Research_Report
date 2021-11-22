using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace STS.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        internal ClaimsIdentity id;
    }
}
