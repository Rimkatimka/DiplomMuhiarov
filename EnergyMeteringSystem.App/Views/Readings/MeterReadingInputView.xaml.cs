using EnergyMeteringSystem.App.Helpers;
using EnergyMeteringSystem.App.ViewModels.Readings;
using EnergyMeteringSystem.Core.Models.DTO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace EnergyMeteringSystem.App.Views.Readings
{
    public partial class MeterReadingInputView : UserControl
    {
        public MeterReadingInputView(UserDto currentUser)
        {
            InitializeComponent();
            DataContext = new MeterReadingInputViewModel(currentUser);
        }

        private void TextBox_PreviewTextInput_Decimal(object sender, TextCompositionEventArgs e)
        {
            InputValidator.RestrictDecimalNumbers(sender, e);
        }

        private void TextBox_PreviewKeyDown_BlockSpace(object sender, KeyEventArgs e)
        {
            InputValidator.BlockSpace(sender, e);
        }

        private void MeterItem_Click(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            if (border?.Tag is MeterForReadingDto selectedMeter)
            {
                var viewModel = DataContext as MeterReadingInputViewModel;
                if (viewModel != null)
                {
                    viewModel.SelectedMeter = selectedMeter;
                }
            }
        }
    }

    // Конвертер для стиля выбранного объекта
    public class SelectedObjectStyleConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool isSelected && isSelected)
            {
                return Application.Current.FindResource("SelectedObjectStyle");
            }
            return null;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }

    // Конвертер для стиля выбранного счетчика
    public class SelectedMeterStyleConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool isSelected && isSelected)
            {
                return Application.Current.FindResource("SelectedMeterStyle");
            }
            return Application.Current.FindResource("MeterItemStyle");
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }
}