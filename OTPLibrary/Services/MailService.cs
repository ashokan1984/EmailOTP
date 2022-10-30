using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using OTPLibrary.DTO;
using OTPLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OTPLibrary.Services
{
    public class MailService : IMailService
    {
        private readonly IMailSettings _mailSettings;
        public MailService(IMailSettings mailSettings)
        {
            _mailSettings = mailSettings;
        }

        public async Task<bool> SendEmailAsync(MailRequest mailRequest)
        {
            try
            {
                var email = new MimeMessage();
                email.Sender = MailboxAddress.Parse(_mailSettings.Mail);
                email.To.Add(MailboxAddress.Parse(mailRequest.ToEmail));
                email.Subject = mailRequest.Subject;
                var builder = new BodyBuilder();
                builder.HtmlBody = mailRequest.Body;
                email.Body = builder.ToMessageBody();
                using (var smtp = new SmtpClient())
                {
                    smtp.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
                    smtp.Authenticate(_mailSettings.Mail, _mailSettings.Password);
                    var res = smtp.SendAsync(email).Result;
                    smtp.Disconnect(true);
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
            
        }
    }
}
