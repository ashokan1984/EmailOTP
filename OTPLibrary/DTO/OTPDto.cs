using System;
using System.Collections.Generic;

namespace OTPLibrary.DTO
{
    public class UserOTPDto
    {
        public int OTP { get; set; }

        public DateTime ExpirationTime { get; set; }
    }

    public class UserDto
    {
        public string Email { get; set; }

        public List<UserOTPDto> OTPList { get; set; }

        public int NumberOfTimesTried { get; set; }
    }
}
