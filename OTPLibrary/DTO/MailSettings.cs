using OTPLibrary.Interfaces;

namespace OTPLibrary.DTO
{
    public class MailSettings : IMailSettings
    {
        public string Mail { get; set; }
        public string DisplayName { get; set; }
        public string Password { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
    }
}
