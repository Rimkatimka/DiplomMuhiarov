using EnergyMeteringSystem.App.Helpers;
using EnergyMeteringSystem.App.ViewModels.Objects;
using System.Windows;

namespace EnergyMeteringSystem.App.Views.Objects
{
    public partial class ConsumptionObjectEditView : Window
    {
        private readonly ConsumptionObjectEditViewModel _viewModel;

        public ConsumptionObjectEditView(ConsumptionObjectEditViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = viewModel;

            // ✅ ПОДПИСКА НА СОБЫТИЯ
            viewModel.OnSaved += (s, e) =>
            {
                DialogResult = true;
                Close();
            };

            viewModel.OnCancelled += (s, e) =>
            {
                DialogResult = false;
                Close();
            };

            // ✅ ОБРАБОТЧИК ЗАКРЫТИЯ ОКНА (через крестик)
            this.Closing += (s, e) =>
            {
                // Если есть изменения и не подтверждено сохранение
                if (_viewModel.HasChanges && DialogResult != true)
                {
                    var result = MessageBox.Show(
                        "Вы уверены, что хотите отменить изменения?\n\nВсе несохраненные данные будут потеряны.",
                        "Отмена редактирования",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.No)
                    {
                        e.Cancel = true; // Отменяем закрытие
                    }
                }
            };
        }

        // ✅ ОБРАБОТЧИК КНОПКИ СОХРАНЕНИЯ (если используете кнопку в XAML)
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SaveCommand.CanExecute(null))
            {
                _viewModel.SaveCommand.Execute(null);
            }
        }

        // ✅ ОБРАБОТЧИК КНОПКИ ОТМЕНЫ
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.HasChanges)
            {
                var result = MessageBox.Show(
                    "Вы уверены, что хотите отменить изменения?\n\nВсе несохраненные данные будут потеряны.",
                    "Отмена редактирования",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _viewModel.CancelCommand.Execute(null);
                }
            }
            else
            {
                _viewModel.CancelCommand.Execute(null);
            }
        }
    }
}