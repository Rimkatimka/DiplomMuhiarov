using System;
using EnergyMeteringSystem.Data.Tests;

namespace EnergyMeteringSystem.LoadTests
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("=========================================");
            Console.WriteLine("НАГРУЗОЧНОЕ ТЕСТИРОВАНИЕ СИСТЕМЫ");
            Console.WriteLine("=========================================");
            Console.WriteLine();

            try
            {
                // Запуск тестов
                LoadTestRunner.RunAllTests();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Критическая ошибка: {ex.Message}");
            }

            Console.WriteLine();
            Console.WriteLine("=========================================");
            Console.WriteLine("Нажмите любую клавишу для выхода...");
            _ = Console.ReadKey();
        }
    }
}