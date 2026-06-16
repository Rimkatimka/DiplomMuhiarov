using EnergyMeteringSystem.App.Helpers;
using System.Windows;

namespace EnergyMeteringSystem.App.Views.Directories
{
    public partial class RegionEditView : Window
    {
        private readonly ViewModels.Directories.RegionEditViewModel _viewModel;

        public RegionEditView()
        {
            InitializeComponent();
            _viewModel = DataContext as ViewModels.Directories.RegionEditViewModel;

            if (_viewModel != null)
            {
                _viewModel.OnRegionSaved += (s, e) => Close();

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