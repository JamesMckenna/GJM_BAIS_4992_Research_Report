using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityManagement.Areas.Identity.Pages.Admin
{
    [Authorize]
    public partial class IndexModel : PageModel
    {
        public IndexModel() {}
        public string Username { get; set; }
        public IActionResult OnGetAsync()
        {
            return Page();
        }
    }
}
