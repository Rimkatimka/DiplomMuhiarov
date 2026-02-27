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
using EnergyMeteringSystem.App.ViewModels.Main;
using EnergyMeteringSystem.Core.Models.DTO;

namespace EnergyMeteringSystem.App.Views.Main
{
    /// <summary>
    /// Логика взаимодействия для ShellView.xaml
    /// </summary>
    public partial class ShellView : Window
    {
        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is MenuItemViewModel menuItem && menuItem.Command != null)
            {
                menuItem.Command.Execute(null);
            }
        }
        public ShellView(UserDto currentUser)
        {
            InitializeComponent();
            DataContext = new ShellViewModel(currentUser);
        }
    }
}
