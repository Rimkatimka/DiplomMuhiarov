using System.Windows.Controls;
using EnergyMeteringSystem.App.ViewModels.Admin;

namespace EnergyMeteringSystem.App.Views.Admin
{
    /// <summary>
    /// Логика взаимодействия для UserEditView.xaml
    /// </summary>
    public partial class UserEditView : UserControl
    {
        public UserEditView(UserEditViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;  // ← устанавливаем DataContext
        }
    }
}

