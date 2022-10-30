using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OTPLibrary.DTO;
using OTPLibrary.Interfaces;
using OTPLibrary.Services;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OTPLibrary.Test
{
    [TestClass]
    public class EmailOTPServiceTest
    {
        private Mock<IMailService> _mailServiceMock;
        private EmailOTPService _emailOTPService;
        private int _otpTimeoutInMinutes = 1;

        [TestInitialize]
        public void Initialize()
        {
            //Arrange
            var inMemorySettings = new Dictionary<string, string> {
                  {"ValidDomain", "dso.org.sg, ethereal.email"},
                   {"MaxTryCount", "10"},
                   {"OTPTimeoutInMinutes", _otpTimeoutInMinutes.ToString()}
                };
            IConfiguration configuration = new ConfigurationBuilder()
    .AddInMemoryCollection(inMemorySettings)
    .Build();
            _mailServiceMock = new Mock<IMailService>();
            _emailOTPService = new EmailOTPService(configuration, _mailServiceMock.Object);
        }

        [TestMethod]
        public void CheckStatus_Pass()
        {
            var userEmail = "test@dso.org.sg";
            _mailServiceMock.Setup(x => x.SendEmailAsync(It.IsAny<MailRequest>())).Returns(Task.FromResult(true));
            var emailSentStatus = _emailOTPService.GenerateOTPEmail(userEmail).Result;

            Assert.AreEqual(EmailStatusEnum.STATUS_EMAIL_OK, emailSentStatus);

            var otp = _emailOTPService.GetUserOtp(userEmail);
            var checkOtpStatus = _emailOTPService.CheckOTP(userEmail, otp);

            Assert.AreEqual(OTPStatusEnum.STATUS_OTP_OK, checkOtpStatus);
        }

        [TestMethod]
        public void CheckStatus_EmailSent_Fail()
        {
            var userEmail = "test@dso.org.sg";
            _mailServiceMock.Setup(x => x.SendEmailAsync(It.IsAny<MailRequest>())).Returns(Task.FromResult(false));
            var emailSentStatus = _emailOTPService.GenerateOTPEmail(userEmail).Result;

            Assert.AreEqual(EmailStatusEnum.STATUS_EMAIL_FAIL, emailSentStatus);
        }

        [TestMethod]
        public void CheckStatus_EmailFail_InvalidDomain()
        {
            var userEmail = "test@gmail.com";
            var emailSentStatus = _emailOTPService.GenerateOTPEmail(userEmail).Result;

            Assert.AreEqual(EmailStatusEnum.STATUS_EMAIL_INVALID, emailSentStatus);
        }

        [TestMethod]
        public void CheckStatus_EmailFail_InvalidEmail()
        {
            var userEmail = "te@st@dso.org.sg";
            _mailServiceMock.Setup(x => x.SendEmailAsync(It.IsAny<MailRequest>())).Returns(Task.FromResult(true));
            var emailSentStatus = _emailOTPService.GenerateOTPEmail(userEmail).Result;

            Assert.AreEqual(EmailStatusEnum.STATUS_EMAIL_INVALID, emailSentStatus);
        }

        [TestMethod]
        public void CheckStatus_Timeout()
        {
            var userEmail = "test@dso.org.sg";
            _mailServiceMock.Setup(x => x.SendEmailAsync(It.IsAny<MailRequest>())).Returns(Task.FromResult(true));
            var emailSentStatus = _emailOTPService.GenerateOTPEmail(userEmail).Result;

            Assert.AreEqual(EmailStatusEnum.STATUS_EMAIL_OK, emailSentStatus);

            var otp = _emailOTPService.GetUserOtp(userEmail);
            Thread.Sleep(60001 * _otpTimeoutInMinutes);
            var checkOtpStatus = _emailOTPService.CheckOTP(userEmail, otp);

            Assert.AreEqual(OTPStatusEnum.STATUS_OTP_TIMEOUT, checkOtpStatus);
        }

        [TestMethod]
        public void CheckStatus_InvalidOtp()
        {
            var userEmail = "test@dso.org.sg";
            _mailServiceMock.Setup(x => x.SendEmailAsync(It.IsAny<MailRequest>())).Returns(Task.FromResult(true));
            var emailSentStatus = _emailOTPService.GenerateOTPEmail(userEmail).Result;

            Assert.AreEqual(EmailStatusEnum.STATUS_EMAIL_OK, emailSentStatus);

            var checkOtpStatus = _emailOTPService.CheckOTP(userEmail, 12347);

            Assert.AreEqual(OTPStatusEnum.STATUS_OTP_FAIL, checkOtpStatus);
        }

        [TestMethod]
        public void CheckStatus_After_MaxTryCount()
        {
            var userEmail = "test@dso.org.sg";
            _mailServiceMock.Setup(x => x.SendEmailAsync(It.IsAny<MailRequest>())).Returns(Task.FromResult(true));
            var emailSentStatus = _emailOTPService.GenerateOTPEmail(userEmail).Result;

            Assert.AreEqual(EmailStatusEnum.STATUS_EMAIL_OK, emailSentStatus);

            var otp = _emailOTPService.GetUserOtp(userEmail);

            for (int i = 0; i < 10; i++)
            {
                var checkOtpStatus = _emailOTPService.CheckOTP(userEmail, 123456);
                Assert.AreEqual(OTPStatusEnum.STATUS_OTP_FAIL, checkOtpStatus);
            }

            var otpStatus = _emailOTPService.CheckOTP(userEmail, otp);
            Assert.AreEqual(OTPStatusEnum.STATUS_OTP_FAIL, otpStatus);
        }

        [TestMethod]
        public void CheckStatus_Before_MaxTryCount()
        {
            var userEmail = "test@dso.org.sg";
            _mailServiceMock.Setup(x => x.SendEmailAsync(It.IsAny<MailRequest>())).Returns(Task.FromResult(true));
            var emailSentStatus = _emailOTPService.GenerateOTPEmail(userEmail).Result;

            Assert.AreEqual(EmailStatusEnum.STATUS_EMAIL_OK, emailSentStatus);

            var otp = _emailOTPService.GetUserOtp(userEmail);

            for (int i = 0; i < 5; i++)
            {
                var checkOtpStatus = _emailOTPService.CheckOTP(userEmail, 123456);
                Assert.AreEqual(OTPStatusEnum.STATUS_OTP_FAIL, checkOtpStatus);
            }

            var otpStatus = _emailOTPService.CheckOTP(userEmail, otp);
            Assert.AreEqual(OTPStatusEnum.STATUS_OTP_OK, otpStatus);
        }

        [TestMethod]
        public void CheckStatus_At_MaxTryCount()
        {
            var userEmail = "test@dso.org.sg";
            _mailServiceMock.Setup(x => x.SendEmailAsync(It.IsAny<MailRequest>())).Returns(Task.FromResult(true));
            var emailSentStatus = _emailOTPService.GenerateOTPEmail(userEmail).Result;

            Assert.AreEqual(EmailStatusEnum.STATUS_EMAIL_OK, emailSentStatus);

            var otp = _emailOTPService.GetUserOtp(userEmail);

            for (int i = 0; i < 9; i++)
            {
                var checkOtpStatus = _emailOTPService.CheckOTP(userEmail, 123456);
                Assert.AreEqual(OTPStatusEnum.STATUS_OTP_FAIL, checkOtpStatus);
            }

            var otpStatus = _emailOTPService.CheckOTP(userEmail, otp);
            Assert.AreEqual(OTPStatusEnum.STATUS_OTP_OK, otpStatus);
        }
    }
}
