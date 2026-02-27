using System.Windows.Controls;
using EnergyMeteringSystem.App.ViewModels.Billing;
using EnergyMeteringSystem.Core.Models.DTO;

namespace EnergyMeteringSystem.App.Views.Billing
{
    /// <summary>
    /// Логика взаимодействия для PaymentView.xaml
    /// </summary>
    public partial class PaymentView : UserControl
    {
        public PaymentView(UserDto currentUser)
        {
            InitializeComponent();
            DataContext = new PaymentViewModel(currentUser);
        }
    }
}
