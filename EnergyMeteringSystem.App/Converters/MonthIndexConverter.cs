using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace EnergyMeteringSystem.App.Converters
{
    public class MonthNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int month)
            {
                string[] months = { "Январь", "Февраль", "Март", "Апрель", "Май", "Июнь",
                                     "Июль", "Август", "Сентябрь", "Октябрь", "Ноябрь", "Декабрь" };
                return months[month - 1];
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
