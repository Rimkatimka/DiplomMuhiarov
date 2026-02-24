using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnergyMeteringSystem.App.Commands;

namespace EnergyMeteringSystem.App.ViewModels.Main
{
    public class MenuItemViewModel
    {
        public string Title { get; set; }
        public RelayCommand Command { get; set; }
        public object Icon { get; set; }
    }
}
