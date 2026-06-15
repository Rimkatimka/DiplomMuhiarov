using EnergyMeteringSystem.App.ViewModels.Objects;
using System.Windows.Controls;

namespace EnergyMeteringSystem.App.Views.Objects
{
    public partial class ConsumptionObjectListView : UserControl
    {
        public ConsumptionObjectListView()
        {
            InitializeComponent();
            DataContext = new ConsumptionObjectListViewModel();
        }
    }
}