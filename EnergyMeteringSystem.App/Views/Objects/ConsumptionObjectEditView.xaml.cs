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

            viewModel.OnSaved += (s, e) => Close();
            viewModel.OnCancelled += (s, e) => Close();

            this.Closing += (s, e) =>
            {
                if (_viewModel.HasChanges && DialogService.ConfirmCancel())
                {
                    e.Cancel = false;
                }
            };
        }
    }
}