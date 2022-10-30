namespace OTPLibrary.Interfaces
{
    public interface IMailSettings
    {
         string Mail { get; set; }
         string DisplayName { get; set; }
         string Password { get; set; }
         string Host { get; set; }
         int Port { get; set; }
    }
}
