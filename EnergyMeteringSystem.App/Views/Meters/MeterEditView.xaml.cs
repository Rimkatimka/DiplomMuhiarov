using System.Windows;
using EnergyMeteringSystem.App.ViewModels.Meters;

namespace EnergyMeteringSystem.App.Views.Meters
{
    /// <summary>
    /// Логика взаимодействия для MeterEditView.xaml
    /// </summary>
    public partial class MeterEditView : Window
    {
        public MeterEditView(MeterEditViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            viewModel.OnMeterSaved += (s, e) => Close();
        }
    }
}
