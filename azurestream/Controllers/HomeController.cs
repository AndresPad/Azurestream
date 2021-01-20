using azurestream.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace azurestream.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        //----------------------------------------------------------------------------------------------------------
        public IActionResult Index()
        {
            return View();
        }

        //----------------------------------------------------------------------------------------------------------
        [Authorize]
        public IActionResult About()
        {
            return View();
        }

        //----------------------------------------------------------------------------------------------------------
        public IActionResult Contact()
        {
            return View();
        }

        //----------------------------------------------------------------------------------------------------------
        public IActionResult Privacy()
        {
            return View();
        }

        //----------------------------------------------------------------------------------------------------------
        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
