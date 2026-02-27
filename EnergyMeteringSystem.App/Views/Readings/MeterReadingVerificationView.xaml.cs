using System.Windows.Controls;

namespace EnergyMeteringSystem.App.Views.Readings
{
    /// <summary>
    /// Логика взаимодействия для MeterReadingVerificationView.xaml
    /// </summary>
    public partial class MeterReadingVerificationView : UserControl
    {
        public MeterReadingVerificationView()
        {
            InitializeComponent();

            DataContext = new ViewModels.Readings.MeterReadingVerificationViewModel();
        }
    }
}
