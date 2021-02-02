using System.Threading.Tasks;

namespace azurestream.Services
{
    //--------------------------------------------------------------------------------------------------------------
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}
