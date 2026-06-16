using System.Windows;

namespace EnergyMeteringSystem.App.Helpers
{
    public static class DialogService
    {
        public static bool Confirm(string message, string title = "Подтверждение")
        {
            return MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
        }

        public static bool ConfirmCancel()
        {
            return Confirm(
                "Вы уверены, что хотите отменить изменения?\n\nВсе несохраненные данные будут потеряны.",
                "Отмена редактирования");
        }

        public static bool ConfirmDelete(string itemName)
        {
            return Confirm(
                $"Вы уверены, что хотите удалить \"{itemName}\"?\n\nЭто действие нельзя отменить.",
                "Подтверждение удаления");
        }

        public static void ShowError(string message, string title = "Ошибка")
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public static void ShowInfo(string message, string title = "Информация")
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}