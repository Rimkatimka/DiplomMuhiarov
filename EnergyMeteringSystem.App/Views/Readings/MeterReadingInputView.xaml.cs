using System.Windows.Controls;
using EnergyMeteringSystem.App.ViewModels.Readings;
using EnergyMeteringSystem.Core.Models.DTO;

namespace EnergyMeteringSystem.App.Views.Readings
{
    /// <summary>
    /// Логика взаимодействия для MeterReadingInputView.xaml
    /// </summary>
    public partial class MeterReadingInputView : UserControl
    {
        public MeterReadingInputView(UserDto currentUser)
        {
            InitializeComponent();
            DataContext = new MeterReadingInputViewModel(currentUser);
        }
    }
}
