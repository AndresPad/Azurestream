using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.IO;
using System.Threading.Tasks;

namespace azurestream.Controllers
{
    //----------------------------------------------------------------------------------------------------------
    public class Xc : Controller
    {
        protected ICompositeViewEngine viewEngine;

        //----------------------------------------------------------------------------------------------------------
        public Xc(ICompositeViewEngine viewEngine)
        {
            this.viewEngine = viewEngine;
        }

        //----------------------------------------------------------------------------------------------------------
        protected async Task<string> RenderViewAsString(object objModel, string viewName = null)
        {
            viewName = viewName ?? ControllerContext.ActionDescriptor.ActionName;
            ViewData.Model = objModel;

            using (StringWriter sw = new StringWriter())
            {
                var viewResult = viewEngine.FindView(ControllerContext, viewName, true).View;
                var viewContext = new ViewContext(ControllerContext, viewResult, ViewData, TempData, sw, new HtmlHelperOptions());
                await viewResult.RenderAsync(viewContext);
                return sw.GetStringBuilder().ToString();
            }
        }
    }
}
