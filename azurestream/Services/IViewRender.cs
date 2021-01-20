using System.Threading.Tasks;

namespace azurestream.Services
{
    //----------------------------------------------------------------------------------------------------------
    public interface IViewRender
    {
        Task<string> RenderToStringAsync(string viewName, object objModel);
    }
}
