using EnergyMeteringSystem.App.Helpers;
using System.Windows;

namespace EnergyMeteringSystem.App.Views.Directories
{
    public partial class CityEditView : Window
    {
        private readonly ViewModels.Directories.CityEditViewModel _viewModel;

        public CityEditView()
        {
            InitializeComponent();
            _viewModel = DataContext as ViewModels.Directories.CityEditViewModel;

            if (_viewModel != null)
            {
                _viewModel.OnCitySaved += (s, e) => Close();

                this.Closing += (s, e) =>
                {
                    if (DialogService.ConfirmCancel())
                    {
                        e.Cancel = false;
                    }
                };
            }
        }
    }
}