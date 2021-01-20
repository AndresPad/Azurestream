using azurestream.Controllers;
using azurestream.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Logging;
using System;

namespace apa.Areas.FishTank.Controllers
{
    //--------------------------------------------------------------------------------------------------------------
    [AllowAnonymous]
    [Area("FishTank")]
    public class FTAdminController : Xc
    {
        private readonly IViewRender _viewRenderService;
        private readonly IViewModelService viewModelService;
        private readonly ILogger _logger;
        //----------------------------------------------------------------------------------------------------------
        public FTAdminController(
            ICompositeViewEngine viewEngine,
            IViewRender viewRenderService,
            IViewModelService viewModelService,
            ILoggerFactory loggerFactory) : base(viewEngine)
        {
            _viewRenderService = viewRenderService;
            this.viewModelService = viewModelService;
            _logger = loggerFactory.CreateLogger<FTAdminController>();
        }

        //----------------------------------------------------------------------------------------------------------
        public IActionResult Index()
        {
            ViewBag.Title = "Fish tank dashboard";
            return View(viewModelService.GetDashboardViewModel());
        }

        //----------------------------------------------------------------------------------------------------------
        public IActionResult Index2()
        {
            ViewBag.Title = "Fish tank dashboard";
            return View(viewModelService.GetDashboardViewModel());
        }

        //----------------------------------------------------------------------------------------------------------
        public IActionResult Feed(int foodAmount)
        {
            var model = viewModelService.GetDashboardViewModel();
            model.LastFed = $"{DateTime.Now.Hour}:{DateTime.Now.Minute}. Amount: {foodAmount}";
            return View("Index", model);
        }
    }
}