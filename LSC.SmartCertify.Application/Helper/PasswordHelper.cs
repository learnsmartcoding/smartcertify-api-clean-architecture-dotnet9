using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSC.SmartCertify.Application.Helper
{
    public static class PasswordHelper
    {
        private static readonly char[] ValidChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()-_=+<>?".ToCharArray();

        public static string GenerateRandomPassword(int length = 12)
        {
            var random = new Random();
            var password = new char[length];

            for (int i = 0; i < length; i++)
            {
                password[i] = ValidChars[random.Next(ValidChars.Length)];
            }

            return new string(password);
        }
    }

}
