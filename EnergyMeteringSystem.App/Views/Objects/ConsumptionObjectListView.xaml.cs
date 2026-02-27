using System.Windows.Controls;
using EnergyMeteringSystem.App.ViewModels.Objects;

namespace EnergyMeteringSystem.App.Views.Objects
{
    /// <summary>
    /// Логика взаимодействия для ConsumptionObjectListView.xaml
    /// </summary>
    public partial class ConsumptionObjectListView : UserControl
    {
        public ConsumptionObjectListView()
        {
            InitializeComponent();
            DataContext = new ConsumptionObjectListViewModel();
        }
    }
}
