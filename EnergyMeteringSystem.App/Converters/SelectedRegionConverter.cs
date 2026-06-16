using System;
using System.Globalization;
using System.Windows.Data;

namespace EnergyMeteringSystem.App.Converters
{
    public class SelectedRegionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            try
            {
                int regionId = (int)value;
                var selectedRegion = parameter as Core.Models.DTO.RegionAnalyticsDto;

                if (selectedRegion == null)
                    return false;

                return regionId == selectedRegion.RegionId;
            }
            catch
            {
                return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}