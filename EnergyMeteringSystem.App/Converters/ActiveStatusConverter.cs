using System;
using System.Globalization;
using System.Windows.Data;

namespace EnergyMeteringSystem.App.Converters
{
    public class ActiveStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool isActive ? isActive ? "Активен" : "Неактивен" : (object)"";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
