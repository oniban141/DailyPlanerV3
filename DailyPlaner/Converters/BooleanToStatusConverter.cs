using System;
using System.Globalization;
using System.Windows.Data;

namespace DailyPlaner.Converters
{
    public class BooleanToStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? "Completed" : "Pending";
            }
            return "Unknown";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue)
            {
                return stringValue.Equals("Completed", StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }
    }
}
