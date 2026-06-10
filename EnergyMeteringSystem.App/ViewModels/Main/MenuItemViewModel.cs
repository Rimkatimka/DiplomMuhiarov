using System.Collections.ObjectModel;
using EnergyMeteringSystem.App.Commands;

namespace EnergyMeteringSystem.App.ViewModels.Main
{
    public class MenuItemViewModel
    {
        public string Title { get; set; }
        public RelayCommand Command { get; set; }
        public ObservableCollection<MenuItemViewModel> Children { get; set; }

        public MenuItemViewModel()
        {
            Children = [];
        }
    }
}
