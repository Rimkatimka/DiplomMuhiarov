using System.Windows.Controls;
using EnergyMeteringSystem.App.ViewModels.Readings;

namespace EnergyMeteringSystem.App.Views.Readings
{
    /// <summary>
    /// Логика взаимодействия для MeterReadingHistoryView.xaml
    /// </summary>
    public partial class MeterReadingHistoryView : UserControl
    {
        public MeterReadingHistoryView()
        {
            InitializeComponent();
            DataContext = new MeterReadingHistoryViewModel();
        }
    }
}
