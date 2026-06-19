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
        /// Путь к активной базе данных.
        /// Приоритет: bin\Database → EnergyMeteringSystem.App\Database → EnergyMeteringSystem.Data\Database
        /// </summary>
        public static string GetActiveDatabasePath()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

            string workingPath = Path.Combine(baseDir, "Database", "EnergyMeteringSystem.mdf");
            if (File.Exists(workingPath))
                return Path.GetFullPath(workingPath);

            try
            {
                string solutionDir = GetSolutionDirectory(baseDir);

                string appProjectPath = Path.Combine(solutionDir, "EnergyMeteringSystem.App", "Database", "EnergyMeteringSystem.mdf");
                if (File.Exists(appProjectPath))
                    return appProjectPath;

                string dataProjectPath = Path.Combine(solutionDir, "EnergyMeteringSystem.Data", "Database", "EnergyMeteringSystem.mdf");
                if (File.Exists(dataProjectPath))
                    return dataProjectPath;
            }
            catch
            {
                // fallback below
            }

            return GetSourceDatabasePath();
        }

        public static string GetActiveDatabaseLogPath()
        {
            string mdfPath = GetActiveDatabasePath();
            string directory = Path.GetDirectoryName(mdfPath) ?? string.Empty;

            string logPath = Path.Combine(directory, "EnergyMeteringSystem_log.ldf");
            if (File.Exists(logPath))
                return logPath;

            logPath = Path.Combine(directory, "EnergyMeteringSystem.ldf");
            if (File.Exists(logPath))
                return logPath;

            return Path.Combine(directory, "EnergyMeteringSystem_log.ldf");
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