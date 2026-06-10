// В папке Converters добавьте:

using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using System;

public class BackgroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (value is bool && (bool)value)
            ? new SolidColorBrush(Color.FromRgb(227, 242, 253))  // Голубой
            : new SolidColorBrush(Colors.White);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class BackgroundMeterConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (value is bool && (bool)value)
            ? new SolidColorBrush(Color.FromRgb(232, 245, 233))  // Зеленый
            : new SolidColorBrush(Color.FromRgb(245, 245, 245)); // Серый
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}