// EnergyMeteringSystem.App/Views/Dynamic/DynamicEditView.xaml.cs
using EnergyMeteringSystem.App.ViewModels.Dynamic;
using System.Windows;

namespace EnergyMeteringSystem.App.Views.Dynamic
{
    public partial class DynamicEditView : Window
    {
        public DynamicEditView(DynamicEditViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            // Подписываемся на события
            viewModel.OnSaved += (s, e) => DialogResult = true;
            viewModel.OnCancelled += (s, e) => DialogResult = false;

            // Закрываем окно после сохранения или отмены
            viewModel.OnSaved += (s, e) => Close();
            viewModel.OnCancelled += (s, e) => Close();
        }
    }
}