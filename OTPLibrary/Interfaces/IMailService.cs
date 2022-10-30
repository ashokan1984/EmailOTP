using OTPLibrary.DTO;
using System.Threading.Tasks;

namespace OTPLibrary.Interfaces
{
    public interface IMailService
    {
        Task<bool> SendEmailAsync(MailRequest mailRequest);
    }
}
