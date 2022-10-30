using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OTPLibrary.DTO;
using OTPLibrary.Interfaces;
using OTPLibrary.Services;
using System;
using System.IO;
using System.Threading;

namespace EmailOTP
{
    class Program
    {
        public static IConfiguration Configuration { get; set; }
        static void Main(string[] args)
        {
            var services = new ServiceCollection();
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appSettings.json");
            Configuration = builder.Build();
            
            //Dependency Injection
            ConfigureServices(services);

            var emailOtpService = services.BuildServiceProvider(false).GetService<IEmailOTPService>();
            var emailStatus = emailOtpService.GenerateOTPEmail("jessie.johns@ethereal.email").Result;
            int receivedOtp = 0;

            Thread.Sleep(66000);
            var checkOtpStatus = emailOtpService.CheckOTP("jessie.johns@ethereal.email", receivedOtp);


            Console.ReadLine();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            var mailSettings = new MailSettings();
            Configuration.GetSection("MailSettings").Bind(mailSettings);
            services.AddSingleton(Configuration);
            services.AddSingleton<IMailSettings>(mailSettings);
            services.AddTransient<IMailService, MailService>();
            services.AddTransient<IEmailOTPService, EmailOTPService>();
        }
    }
}
