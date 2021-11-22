using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MvcClient.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace MvcClient.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        public async Task Login()
        {
            try
            {
                await HttpContext.ChallengeAsync("oidc", new AuthenticationProperties { RedirectUri = "Home/Index" });
            }
            catch (Exception ex)
            {
                _logger.LogError("~/Account/Login - an error occurred with the ChallengeAsync: {0}", ex);
                throw;
            }
        }

        public IActionResult Logout()
        {
            return SignOut("Cookies", "oidc");
        }

        public async Task<IActionResult> CallAPI()
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");

            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var content = await client.GetStringAsync("https://localhost:6001/developer");

            ViewBag.Json = JArray.Parse(content).ToString();
            return View("json");
        }

        public IActionResult ManageAccount()
        {
            return Redirect("https://localhost:5005/Identity/Account/Manage/Index");
        }


        public async Task<IActionResult> GetUserInfo()
        {
            var user_access_token = await HttpContext.GetUserAccessTokenAsync();

            var client = new HttpClient();

            var disco = await client.GetDiscoveryDocumentAsync("https://localhost:5001");

            var response = await client.GetUserInfoAsync(new UserInfoRequest
            {
                Address = disco.UserInfoEndpoint,
                Token = user_access_token
            });

            ViewBag.claims = response.Claims;
            return View("userInfo");
        }
    }
}
