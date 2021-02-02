using azurestream.Controllers;
using azurestream.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Logging;

namespace azurestream.Areas.CosmosDB.Controllers
{
    //--------------------------------------------------------------------------------------------------------------
    [AllowAnonymous]
    [Area("CosmosDB")]
    public class CosmosAdminController : Xc
    {
        private readonly IViewRender _viewRenderService;
        private readonly IViewModelService viewModelService;
        private readonly ILogger _logger;
        //----------------------------------------------------------------------------------------------------------
        public CosmosAdminController(
            ICompositeViewEngine viewEngine,
            IViewRender viewRenderService,
            IViewModelService viewModelService,
            ILoggerFactory loggerFactory) : base(viewEngine)
        {
            _viewRenderService = viewRenderService;
            this.viewModelService = viewModelService;
            _logger = loggerFactory.CreateLogger<CosmosAdminController>();
        }

        //----------------------------------------------------------------------------------------------------------
        public IActionResult Index()
        {
            ViewBag.Title = "CosmosDB Dashboard";
            return View();
        }

        //----------------------------------------------------------------------------------------------------------
        public IActionResult CosmosToDoList()
        {
            ViewBag.Title = "CosmosDB To Do List";
            return View();
        }
    }
}
