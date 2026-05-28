using System;
using System.IO;
using System.Reflection;

namespace EnergyMeteringSystem.App.Helpers
{
    public static class DatabasePathHelper
    {
        /// <summary>
        /// Возвращает путь к папке с базой данных в проекте EnergyMeteringSystem.Data
        /// </summary>
        public static string GetSourceDatabaseDirectory()
        {
            // Начинаем с папки, где находится исполняемый файл
            string currentDir = AppDomain.CurrentDomain.BaseDirectory;

            // Поднимаемся до корня решения
            string solutionDir = GetSolutionDirectory(currentDir);

            // Формируем путь к папке Database в проекте Data
            string dbDirectory = Path.Combine(solutionDir, "EnergyMeteringSystem.Data", "Database");

            return dbDirectory;
        }

        /// <summary>
        /// Полный путь к MDF файлу в проекте данных
        /// </summary>
        public static string GetSourceDatabasePath()
        {
            return Path.Combine(GetSourceDatabaseDirectory(), "EnergyMeteringSystem.mdf");
        }

        /// <summary>
        /// Полный путь к LDF файлу в проекте данных
        /// </summary>
        public static string GetSourceDatabaseLogPath()
        {
            return Path.Combine(GetSourceDatabaseDirectory(), "EnergyMeteringSystem_log.ldf");
        }

        /// <summary>
        /// Путь к рабочей копии базы в bin\Debug\Database\
        /// </summary>
        public static string GetWorkingDatabaseDirectory()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database");
        }

        /// <summary>
        /// Полный путь к рабочей MDF
        /// </summary>
        public static string GetWorkingDatabasePath()
        {
            return Path.Combine(GetWorkingDatabaseDirectory(), "EnergyMeteringSystem.mdf");
        }

        /// <summary>
        /// Полный путь к рабочей LDF
        /// </summary>
        public static string GetWorkingDatabaseLogPath()
        {
            return Path.Combine(GetWorkingDatabaseDirectory(), "EnergyMeteringSystem_log.ldf");
        }

        /// <summary>
        /// Поиск директории решения (где лежит .sln файл)
        /// </summary>
        private static string GetSolutionDirectory(string startPath)
        {
            var directory = new DirectoryInfo(startPath);

            while (directory != null && directory.GetFiles("*.sln").Length == 0)
            {
                directory = directory.Parent;
            }

            if (directory == null)
                throw new DirectoryNotFoundException("Не найден файл решения (.sln)");

            return directory.FullName;
        }
    }
}