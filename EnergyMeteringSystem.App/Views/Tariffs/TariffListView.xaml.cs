using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
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
