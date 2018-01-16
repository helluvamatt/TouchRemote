using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace TouchRemote.Utils.Converters
{
    internal class EnumerableNullReplaceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var collection = value as IEnumerable;
            if (collection == null) return null;
            return collection.Cast<object>().Select(x => x ?? parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
