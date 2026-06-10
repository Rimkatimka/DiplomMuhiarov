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
            System.Diagnostics.Debug.WriteLine($"HashPassword: входной пароль = '{password}'");

            // Вариант 1: UTF8
            byte[] utf8Bytes = Encoding.UTF8.GetBytes(password);
            string utf8Hash = Convert.ToBase64String(SHA256.Create().ComputeHash(utf8Bytes));
            System.Diagnostics.Debug.WriteLine($"UTF8 хэш: {utf8Hash}");

            // Вариант 2: Unicode (UTF16)
            byte[] unicodeBytes = Encoding.Unicode.GetBytes(password);
            string unicodeHash = Convert.ToBase64String(SHA256.Create().ComputeHash(unicodeBytes));
            System.Diagnostics.Debug.WriteLine($"Unicode хэш: {unicodeHash}");

            // Вариант 3: ASCII
            byte[] asciiBytes = Encoding.ASCII.GetBytes(password);
            string asciiHash = Convert.ToBase64String(SHA256.Create().ComputeHash(asciiBytes));
            System.Diagnostics.Debug.WriteLine($"ASCII хэш: {asciiHash}");

            // Возвращаем тот, который совпадает с БД
            return utf8Hash;
        }

        /// <summary>
        /// Проверка пароля
        /// </summary>
        public static bool VerifyPassword(string password, string hash)
        {
            string hashOfInput = HashPassword(password);

            // Убираем trailing '=' для сравнения
            hashOfInput = hashOfInput.TrimEnd('=');
            hash = hash.TrimEnd('=');

            return hashOfInput == hash;
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