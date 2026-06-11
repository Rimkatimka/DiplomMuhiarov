using EnergyMeteringSystem.App.ViewModels.Objects;
using System.Windows;

namespace EnergyMeteringSystem.App.Views.Objects
{
    public partial class ConsumptionObjectEditView : Window
    {
        public ConsumptionObjectEditView(ConsumptionObjectEditViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            viewModel.OnObjectSaved += (s, e) => Close();
        }
    }
}