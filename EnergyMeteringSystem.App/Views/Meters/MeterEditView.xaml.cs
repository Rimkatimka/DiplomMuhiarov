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
using EnergyMeteringSystem.App.ViewModels.Meters;

namespace EnergyMeteringSystem.App.Views.Meters
{
    /// <summary>
    /// Логика взаимодействия для MeterEditView.xaml
    /// </summary>
    public partial class MeterEditView : Window
    {
        public MeterEditView(MeterEditViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            viewModel.OnMeterSaved += (s, e) => this.Close();
        }
    }
}
