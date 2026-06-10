using System.Windows.Controls;
using EnergyMeteringSystem.App.ViewModels.Objects;
using EnergyMeteringSystem.Core.Models.DTO;

namespace EnergyMeteringSystem.App.Views.Objects
{
    /// <summary>
    /// Логика взаимодействия для ObjectMetersView.xaml
    /// </summary>
    public partial class ObjectMetersView : UserControl
    {
        public ObjectMetersView(ConsumptionObjectDto selectedObject)
        {
            InitializeComponent();
            if (selectedObject == null)
            {
                return;
            }

            // Создаем ViewModel и устанавливаем DataContext
            ObjectMetersViewModel viewModel = new(selectedObject);
            DataContext = viewModel;

            System.Diagnostics.Debug.WriteLine($"ObjectMetersView создан для объекта: {selectedObject.Address}");
        }
    }
}
