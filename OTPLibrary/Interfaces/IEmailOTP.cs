using OTPLibrary.DTO;
using System.Threading.Tasks;

namespace OTPLibrary.Interfaces
{
    public interface IEmailOTPService
    {
        Task<EmailStatusEnum> GenerateOTPEmail(string userEmail);

        OTPStatusEnum CheckOTP(string userEmail, int otp);
    }
}
