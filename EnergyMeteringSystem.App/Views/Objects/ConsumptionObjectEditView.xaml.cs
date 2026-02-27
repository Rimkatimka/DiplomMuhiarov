using System.Windows;
using EnergyMeteringSystem.App.ViewModels.Objects;

namespace EnergyMeteringSystem.App.Views.Objects
{
    /// <summary>
    /// Логика взаимодействия для ConsumptionObjectEditView.xaml
    /// </summary>
    public partial class ConsumptionObjectEditView : Window
    {
        public ConsumptionObjectEditView(ConsumptionObjectEditViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            // Закрыть окно при успешном сохранении
            viewModel.OnObjectSaved += (s, e) => Close();
        }
    }
}
