using System.Windows.Controls;
using EnergyMeteringSystem.App.ViewModels.Billing;

namespace EnergyMeteringSystem.App.Views.Billing
{
    /// <summary>
    /// Логика взаимодействия для AccrualView.xaml
    /// </summary>
    public partial class AccrualView : UserControl
    {
        public AccrualView()
        {
            InitializeComponent();
            DataContext = new AccrualViewModel();
        }
    }
}
