using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace TouchRemote.Utils.Converters
{
    [ValueConversion(typeof(string), typeof(Visibility))]
    internal class StringIsNullOrEmptyToVisibilityConverter : IValueConverter
    {
        public StringIsNullOrEmptyToVisibilityConverter()
        {
            NotEmpty = Visibility.Visible;
            Empty = Visibility.Collapsed;
        }

        public Visibility NotEmpty { get; set; }

        public Visibility Empty { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.IsNullOrEmpty(value as string) ? Empty : NotEmpty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Not used
            return "";
        }
    }
}
