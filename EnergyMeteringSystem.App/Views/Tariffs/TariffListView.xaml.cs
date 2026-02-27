using System.Windows.Controls;
using EnergyMeteringSystem.App.ViewModels.Tariffs;

namespace EnergyMeteringSystem.App.Views.Tariffs
{
    /// <summary>
    /// Логика взаимодействия для TariffListView.xaml
    /// </summary>
    public partial class TariffListView : UserControl
    {
        public TariffListView()
        {
            InitializeComponent();
            DataContext = new TariffListViewModel();
        }
    }
}
