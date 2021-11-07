using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityAPI.Controllers
{
    public class HomeController : Controller
    {
        //[Route("home")]
        [AllowAnonymous]
        public IActionResult Index()
        {
            return View("Index");
        }
    }
}
