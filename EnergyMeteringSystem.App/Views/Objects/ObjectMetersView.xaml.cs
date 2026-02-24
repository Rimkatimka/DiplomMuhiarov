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
using EnergyMeteringSystem.App.ViewModels.Objects;
using EnergyMeteringSystem.Core.Models.DTO;

namespace EnergyMeteringSystem.App.Views.Objects
{
    /// <summary>
    /// Логика взаимодействия для ObjectMetersView.xaml
    /// </summary>
    public partial class ObjectMetersView : Window
    {
        public ObjectMetersView(ConsumptionObjectDto selectedObject)
        {
            InitializeComponent();
            var vm = new ObjectMetersViewModel(selectedObject);
            vm.CloseRequested += (s, e) => this.Close();
            DataContext = vm;
        }
    }
}
