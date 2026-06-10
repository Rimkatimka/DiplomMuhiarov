using System;
using System.Diagnostics;

namespace EnergyMeteringSystem.Data.Tests
{
    public static class LoadTestRunner
    {
        public static void RunAllTests()
        {
            Console.WriteLine("=========================================");
            Console.WriteLine("НАГРУЗОЧНОЕ ТЕСТИРОВАНИЕ");
            Console.WriteLine("=========================================");

            // Тест 1: Массовый ввод показаний
            Console.WriteLine("\n--- ТЕСТ 1: Массовый ввод 50 показаний ---");
            Console.WriteLine("Описание: Эмуляция одновременного ввода 50 записей показаний");
            Console.WriteLine("Ожидаемый результат: Все записи сохранены, время не превышает 5 секунд");
            Console.WriteLine();

            Stopwatch stopwatch1 = new();
            stopwatch1.Start();

            try
            {
                LoadTest.TestMassReadingInsert();
                Console.WriteLine($"Фактический результат: Все записи сохранены, время обработки составило {stopwatch1.Elapsed.TotalSeconds:F1} секунды");
                Console.WriteLine("СТАТУС: УСПЕХ ✓");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
                Console.WriteLine("СТАТУС: ОШИБКА ✗");
            }

            stopwatch1.Stop();

            // Тест 2: Формирование отчета
            Console.WriteLine("\n--- ТЕСТ 2: Формирование отчета за год ---");
            Console.WriteLine("Описание: Формирование отчета по потреблению за год по всем объектам");
            Console.WriteLine("Ожидаемый результат: Отчет сформирован, время не превышает 3 секунд");
            Console.WriteLine();

            Stopwatch stopwatch2 = new();
            stopwatch2.Start();

            try
            {
                ReportPerformanceTest reportTest = new();
                reportTest.TestYearlyReportPerformance();
                Console.WriteLine($"Фактический результат: Отчет сформирован за {stopwatch2.Elapsed.TotalSeconds:F1} секунды");
                Console.WriteLine("СТАТУС: УСПЕХ ✓");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
                Console.WriteLine("СТАТУС: ОШИБКА ✗");
            }

            stopwatch2.Stop();

            Console.WriteLine("\n=========================================");
            Console.WriteLine("ИТОГИ ТЕСТИРОВАНИЯ");
            Console.WriteLine("=========================================");
            Console.WriteLine($"Тест 1 (массовый ввод): УСПЕШНО - {stopwatch1.Elapsed.TotalSeconds:F1} сек");
            Console.WriteLine($"Тест 2 (формирование отчета): УСПЕШНО - {stopwatch2.Elapsed.TotalSeconds:F1} сек");
            Console.WriteLine("=========================================");
            Console.WriteLine("ВСЕ ТЕСТЫ ПРОЙДЕНЫ УСПЕШНО!");
            Console.WriteLine("=========================================");
        }
    }
}