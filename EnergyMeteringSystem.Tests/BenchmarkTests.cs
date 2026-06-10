using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using EnergyMeteringSystem.Core.Helpers;
using EnergyMeteringSystem.Data.Repositories;
using System.Text.RegularExpressions;

namespace EnergyMeteringSystem.Tests
{
    [MemoryDiagnoser]
    public class BenchmarkTests
    {
        private readonly string _testPassword = "12345";
        private readonly string _testEmail = "user@example.com";

        // Тест 1: Хэширование пароля
        [Benchmark]
        public string HashPasswordBenchmark()
        {
            return PasswordHelper.HashPassword(_testPassword);
        }

        // Тест 2: Проверка пароля
        [Benchmark]
        public bool VerifyPasswordBenchmark()
        {
            string hash = PasswordHelper.HashPassword(_testPassword);
            return PasswordHelper.VerifyPassword(_testPassword, hash);
        }

        // Тест 3: Валидация email (Regex)
        [Benchmark]
        public bool ValidateEmailBenchmark()
        {
            string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            return Regex.IsMatch(_testEmail, pattern);
        }

        // Тест 4: Получение объектов из БД
        [Benchmark]
        public int GetObjectsBenchmark()
        {
            var repo = new ConsumptionObjectRepository();
            return repo.GetAll().Count;
        }

        // Тест 5: Получение пользователей из БД
        [Benchmark]
        public int GetUsersBenchmark()
        {
            var repo = new UserRepository();
            return repo.GetAll().Count;
        }
    }

    public class RunBenchmarks
    {
        public static void Run()
        {
            var summary = BenchmarkRunner.Run<BenchmarkTests>();
        }
    }
}