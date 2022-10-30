using Microsoft.Extensions.Configuration;
using OTPLibrary.DTO;
using OTPLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OTPLibrary.Services
{
    public class EmailOTPService : IDisposable, IEmailOTPService
    {
        private Dictionary<string, UserDto> _otpDictionary;
        private readonly IConfiguration _configuration;
        private readonly IMailService _mailService;

        public EmailOTPService(IConfiguration configuration, IMailService mailService)
        {
            _otpDictionary = new Dictionary<string, UserDto>();
            _configuration = configuration;
            _mailService = mailService;
        }



        /// <summary>
        /// Generate OTP and send OTP in email. OTP is valid for 1 minute
        /// </summary>
        public async Task<EmailStatusEnum> GenerateOTPEmail(string userEmail)
        {
            var validDomain = _configuration["ValidDomain"].Split(',').Select(s => s.ToLower().Trim());
            if (IsValidEmail(userEmail) && validDomain.Contains(userEmail.Split('@')[1].ToLowerInvariant()))
            {
                var otp = GenerateRandomNumber();
                var otpTimeout = Convert.ToInt32(_configuration["OTPTimeoutInMinutes"]);
                var mailRequest = new MailRequest
                {
                    Body = "Your OTP Code is " + otp + ". The code is valid for " + otpTimeout + " minute",
                    Subject = "OTP for Orange Business",
                    ToEmail = userEmail
                };
                var isSent = await _mailService.SendEmailAsync(mailRequest);
                //TODO : Need to check email delivered before setting expiration time for generated OTP
                if (isSent)
                {
                    var userOtpDto = new UserOTPDto
                    {
                        ExpirationTime = DateTime.UtcNow.AddMinutes(otpTimeout),
                        OTP = otp
                    };

                    if (!_otpDictionary.ContainsKey(userEmail.ToLower()))
                        _otpDictionary.Add(userEmail.ToLower(), new UserDto { Email = userEmail, NumberOfTimesTried = 0, OTPList = new List<UserOTPDto> { userOtpDto } });
                    else
                        _otpDictionary[userEmail.ToLower()].OTPList.Add(userOtpDto);

                    return EmailStatusEnum.STATUS_EMAIL_OK;
                }
                else
                {
                    return EmailStatusEnum.STATUS_EMAIL_FAIL;
                }
            }
            else
            {
                return EmailStatusEnum.STATUS_EMAIL_INVALID;
            }
        }

        /// <summary>
        /// Validate OTP
        /// 1. Expiration time one minute
        /// 2. Maxmimum 10 times user can try
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public OTPStatusEnum CheckOTP(string userEmail, int otp)
        {
            int maxTryCount = Convert.ToInt32(_configuration["MaxTryCount"]);
            if(_otpDictionary[userEmail].OTPList.Last().OTP == otp && _otpDictionary[userEmail].NumberOfTimesTried < maxTryCount)
            {
                if(DateTime.UtcNow <= _otpDictionary[userEmail].OTPList.Last().ExpirationTime)
                {
                    return OTPStatusEnum.STATUS_OTP_OK;
                }
                else
                {
                    return OTPStatusEnum.STATUS_OTP_TIMEOUT;
                }
            }
            else
            {
                _otpDictionary[userEmail].NumberOfTimesTried++;
                return OTPStatusEnum.STATUS_OTP_FAIL;
            }
        }

        public int GetUserOtp(string userEmail)
        {
            return _otpDictionary[userEmail].OTPList.Last().OTP;
        }

        /// <summary>
        /// Generate six digit random number
        /// </summary>
        /// <returns></returns>
        private static int GenerateRandomNumber()
        {
            var rand = new Random();
            var otp = rand.Next(100000, 999999);
            return otp;
        }

        private static bool IsValidEmail(string email)
        {
            var trimmedEmail = email.Trim();

            if (trimmedEmail.EndsWith("."))
            {
                return false; // suggested by @TK-421
            }
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == trimmedEmail;
            }
            catch
            {
                return false;
            }
        }

        public void Dispose()
        {
            _otpDictionary = null;
        }
    }
}
