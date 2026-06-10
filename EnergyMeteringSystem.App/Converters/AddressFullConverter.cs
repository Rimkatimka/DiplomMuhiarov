using System;
using System.Globalization;
using System.Windows.Data;

namespace EnergyMeteringSystem.App.Converters
{
    public class AddressFullConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 6)
                return "Адрес не указан";

            string region = values[0] as string ?? "";
            string city = values[1] as string ?? "";
            string street = values[2] as string ?? "";
            string houseNumber = values[3] as string ?? "";
            string apartmentNumber = values[4] as string ?? "";
            string originalAddress = values[5] as string ?? "";

            // Если есть полный адрес из БД - используем его
            if (!string.IsNullOrEmpty(originalAddress) && originalAddress != "Адрес не указан")
                return originalAddress;

            // Формируем адрес из частей
            string result = "";

            if (!string.IsNullOrEmpty(region))
                result += region + ", ";
            if (!string.IsNullOrEmpty(city))
                result += city + ", ";
            if (!string.IsNullOrEmpty(street))
                result += street + ", ";
            if (!string.IsNullOrEmpty(houseNumber))
                result += "д. " + houseNumber;
            if (!string.IsNullOrEmpty(apartmentNumber))
                result += ", кв. " + apartmentNumber;

            // Если ничего не получилось
            if (string.IsNullOrEmpty(result))
                return originalAddress;

            return result;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}