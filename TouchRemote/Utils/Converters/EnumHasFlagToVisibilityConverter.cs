using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TouchRemote.Utils.Converters
{
    [ValueConversion(typeof(Enum), typeof(Visibility))]
    internal class EnumHasFlagToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((Enum)value).HasFlag((Enum)parameter) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
