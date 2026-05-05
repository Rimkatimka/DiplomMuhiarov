using System.Windows.Controls;
using EnergyMeteringSystem.App.ViewModels.Reports;

namespace EnergyMeteringSystem.App.Views.Reports
{
    /// <summary>
    /// Логика взаимодействия для ReportView.xaml
    /// </summary>
    public partial class ReportView : UserControl
    {
        public ReportView()
        {
            InitializeComponent();
            DataContext = new ReportViewModel();
        }
    }
}
