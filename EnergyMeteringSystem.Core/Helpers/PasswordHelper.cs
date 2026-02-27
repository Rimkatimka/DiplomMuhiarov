using System;
using System.Security.Cryptography;
using System.Text;

namespace EnergyMeteringSystem.Core.Helpers
{
    public static class PasswordHelper
    {
        /// <summary>
        /// Хэширование пароля (SHA256)
        /// </summary>
        public static string HashPassword(string password)
        {
            using SHA256 sha256 = SHA256.Create();
            byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        /// <summary>
        /// Проверка пароля
        /// </summary>
        public static bool VerifyPassword(string password, string hash)
        {
            return HashPassword(password) == hash;
        }

        /// <summary>
        /// Генерация случайного пароля
        /// </summary>
        public static string GenerateRandomPassword(int length = 8)
        {
            const string chars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            Random random = new();
            StringBuilder result = new(length);

            for (int i = 0; i < length; i++)
            {
                _ = result.Append(chars[random.Next(chars.Length)]);
            }

            return result.ToString();
        }
    }
}