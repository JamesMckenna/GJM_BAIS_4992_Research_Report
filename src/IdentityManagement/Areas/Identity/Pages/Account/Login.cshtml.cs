using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace IdentityManagement.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        public LoginModel() { }

        public async Task OnGetAsync(string returnUrl = null)
        {

            await HttpContext.ChallengeAsync("oidc", new AuthenticationProperties { RedirectUri = "https://localhost:5005" });
        }
    }
}
