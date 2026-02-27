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

namespace EnergyMeteringSystem.App.Views.Objects
{
    /// <summary>
    /// Логика взаимодействия для ConsumptionObjectEditView.xaml
    /// </summary>
    public partial class ConsumptionObjectEditView : Window
    {
        public ConsumptionObjectEditView(ConsumptionObjectEditViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            // Закрыть окно при успешном сохранении
            viewModel.OnObjectSaved += (s, e) => this.Close();
        }
    }
}
