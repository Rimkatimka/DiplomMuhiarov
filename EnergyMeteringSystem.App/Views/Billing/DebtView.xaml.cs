using System.Windows.Controls;
using EnergyMeteringSystem.App.ViewModels.Billing;

namespace EnergyMeteringSystem.App.Views.Billing
{
    /// <summary>
    /// Логика взаимодействия для DebtView.xaml
    /// </summary>
    public partial class DebtView : UserControl
    {
        public DebtView()
        {
            InitializeComponent();
            System.Diagnostics.Debug.WriteLine("DebtView: конструктор");

            // Убедимся, что DataContext установлен
            DataContext = new DebtViewModel();

            System.Diagnostics.Debug.WriteLine("DebtView: DataContext установлен");
        }
    }
}
