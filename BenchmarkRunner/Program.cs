using System;
using BenchmarkDotNet.Running;
using EnergyMeteringSystem.Tests;

namespace BenchmarkRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== ЗАПУСК БЕНЧМАРКОВ ===\n");

            // Используем полное имя, чтобы указать, что это класс из BenchmarkDotNet
            var summary = BenchmarkDotNet.Running.BenchmarkRunner.Run<BenchmarkTests>();

            Console.WriteLine("\nНажмите любую клавишу для выхода...");
            Console.ReadKey();
        }
    }
}