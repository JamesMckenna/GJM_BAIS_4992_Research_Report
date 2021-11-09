using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AnAPI.Controllers
{
    public class HomeController : Controller
    {
        [Route("home")]
        [Authorize]
        public IActionResult Index()
        {
            return View("Index");
        }
    }
}
