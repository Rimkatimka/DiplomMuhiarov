using System.Windows;
using EnergyMeteringSystem.App.ViewModels.Directories;

namespace EnergyMeteringSystem.App.Views.Directories
{
    /// <summary>
    /// Логика взаимодействия для DirectoryEditView.xaml
    /// </summary>
    public partial class DirectoryEditView : Window
    {
        public DirectoryEditView(DirectoryEditViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            viewModel.OnDirectorySaved += (s, e) => Close();
        }
    }
}
