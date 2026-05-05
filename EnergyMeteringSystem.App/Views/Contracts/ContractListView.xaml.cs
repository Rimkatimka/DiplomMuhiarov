using EnergyMeteringSystem.App.ViewModels.Contracts;
using System.Windows.Controls;

namespace EnergyMeteringSystem.App.Views.Contracts
{
    /// <summary>
    /// Логика взаимодействия для ContractListView.xaml
    /// </summary>
    public partial class ContractListView : UserControl
    {
        public ContractListView()
        {
            InitializeComponent();
            DataContext = new ContractListViewModel();
        }
    }
}
