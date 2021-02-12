using apa.BOL;
using apa.BOL.Config;
using azurestream.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;

namespace azurestream.Controllers
{
    //--------------------------------------------------------------------------------------------------------------
    [AllowAnonymous]
    public class HomeController : Xc
    {
        private readonly IViewRender _viewRenderService;
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;
        private readonly ApplicationInsights _settings;
        private readonly CloudStorageConfig _storageConfig;
        
        private IDistributedCache _cache;
        EmailConfig _emailConfig { get; }
        //----------------------------------------------------------------------------------------------------------
        public HomeController(IDistributedCache cache, IOptions<EmailConfig> emailConfig, IOptions<ApplicationInsights> settings, IOptions<CloudStorageConfig> storageConfig, ICompositeViewEngine viewEngine, IViewRender viewRenderService, IEmailSender emailSender, ILogger<HomeController> logger) : base(viewEngine)
        {
            _viewRenderService = viewRenderService;
            _emailConfig = emailConfig.Value;
            _emailSender = emailSender;
            _storageConfig = storageConfig.Value;
            _logger = logger;
            _settings = settings.Value;

            _cache = cache;

            _logger.LogCritical(5, "Home Controller Critical Error");
        }

        //----------------------------------------------------------------------------------------------------------
        public IActionResult Index()
        {
            string cachetimevalue = _cache.GetString("CacheTime");
            string schoolvalue = _cache.GetString("school");
            string soccervalue = _cache.GetString("Soccer");

            int num = new Random().Next();
            num.ToString();

            //_cache.Remove("CacheTime");
            //_cache.Remove("school");

            if (!string.IsNullOrEmpty(cachetimevalue))
            {
                ViewData["CurrentTime"] = "Fetched from cache : " + cachetimevalue;
            }
            else
            {
                cachetimevalue = DateTime.UtcNow.ToString();
                var options = new DistributedCacheEntryOptions();
                //options.SetSlidingExpiration(TimeSpan.FromMinutes(1));
                options.SetAbsoluteExpiration(new System.TimeSpan(0, 0, 15));
                _cache.SetString("CacheTime", cachetimevalue, options);

                ViewData["CurrentTime"] = "Added to cache : " + cachetimevalue;
            }

            if (!string.IsNullOrEmpty(schoolvalue))
            {
                ViewData["school"] = "Fetched from cache : " + schoolvalue;
            }
            else
            {
                schoolvalue = "Bellows College " + num;
                var options = new DistributedCacheEntryOptions();
                //options.SetSlidingExpiration(TimeSpan.FromMinutes(1));
                options.SetAbsoluteExpiration(new System.TimeSpan(0, 0, 15));
                _cache.SetString("school", schoolvalue, options);

                ViewData["school"] = "Added to cache : " + schoolvalue;
            }

            if (!string.IsNullOrEmpty(soccervalue))
            {
                ViewData["soccer"] = "Fetched from cache : " + soccervalue;
            }
            else
            {
                soccervalue = "Messi";
                _cache.SetString("soccer", schoolvalue);

                ViewData["soccer"] = "Added to cache : " + soccervalue;
            }

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
