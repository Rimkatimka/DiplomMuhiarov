using Microsoft.VisualStudio.TestTools.UnitTesting;
using EnergyMeteringSystem.Core.Helpers;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Repositories;
using System;
using System.Linq;
using BenchmarkDotNet.Running;

namespace EnergyMeteringSystem.Tests
{
    [TestClass]
    public class UnitTests
    {
        // Тест 1: Хэширование пароля
        [TestMethod]
        public void HashPassword_ValidPassword_ReturnsHash()
        {
            // Arrange
            string password = "12345";

            // Act
            string hash = PasswordHelper.HashPassword(password);

            // Assert
            Assert.IsNotNull(hash);
            Assert.AreNotEqual(password, hash);
            Assert.IsTrue(hash.Length >= 40);
        }

        // Тест 2: Проверка правильного пароля
        [TestMethod]
        public void VerifyPassword_CorrectPassword_ReturnsTrue()
        {
            // Arrange
            string password = "12345";
            string hash = PasswordHelper.HashPassword(password);

            // Act
            bool result = PasswordHelper.VerifyPassword(password, hash);

            // Assert
            Assert.IsTrue(result);
        }

        // Тест 3: Проверка неправильного пароля
        [TestMethod]
        public void VerifyPassword_WrongPassword_ReturnsFalse()
        {
            // Arrange
            string password = "12345";
            string wrongPassword = "wrongpassword";
            string hash = PasswordHelper.HashPassword(password);

            // Act
            bool result = PasswordHelper.VerifyPassword(wrongPassword, hash);

            // Assert
            Assert.IsFalse(result);
        }

        // Тест 4: Получение всех пользователей из БД
        [TestMethod]
        public void GetAllUsers_ReturnsList()
        {
            // Arrange
            var repo = new UserRepository();

            // Act
            var users = repo.GetAll();

            // Assert
            Assert.IsNotNull(users);
            Assert.IsTrue(users.Count > 0, "В БД нет пользователей");

            // Вывод для отладки
            System.Diagnostics.Debug.WriteLine($"Найдено пользователей: {users.Count}");
            foreach (var user in users.Take(3))
            {
                System.Diagnostics.Debug.WriteLine($"  - {user.Username} ({user.RoleText})");
            }
        }

        // Тест 5: Получение всех объектов из БД
        [TestMethod]
        public void GetAllObjects_ReturnsList()
        {
            // Arrange
            var repo = new ConsumptionObjectRepository();

            // Act
            var objects = repo.GetAll();

            // Assert
            Assert.IsNotNull(objects);

            System.Diagnostics.Debug.WriteLine($"Найдено объектов: {objects.Count}");
        }

        [TestMethod]
        public void RunOriginalBenchmarks()
        {
            BenchmarkRunner.Run<BenchmarkTests>();
        }
    }
}