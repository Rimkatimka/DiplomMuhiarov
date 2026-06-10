using System.Windows.Controls;

namespace EnergyMeteringSystem.App.Views.Meters
{
    /// <summary>
    /// Логика взаимодействия для MeterListView.xaml
    /// </summary>
    public partial class MeterListView : UserControl
    {
        public MeterListView()
        {
            InitializeComponent();
            DataContext = new ViewModels.Meters.MeterListViewModel();
        }
    }
}
