using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AzureMapsWebApiToken.Models;
using Microsoft.AspNetCore.Http;

namespace AzureMapsWebApiToken.Controllers
{
    public class HomeController : Controller
    {
        private readonly TokenAuthorizationProvider provider;

        public HomeController(TokenAuthorizationProvider provider)
        {
            this.provider = provider;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Maps()
        {
            var option = new CookieOptions();
            var token = provider.CreateToken();
            option.Expires = token.ValidTo;         
            Response.Cookies.Append("Authorization", token.RawData, option);
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

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
    }
}
