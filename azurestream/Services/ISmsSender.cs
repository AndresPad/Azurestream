using System.Threading.Tasks;

namespace azurestream.Services
{
    //--------------------------------------------------------------------------------------------------------------
    public interface ISmsSender
    {
        Task SendSmsAsync(string number, string message);
    }
}
