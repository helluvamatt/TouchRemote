using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace TouchRemote.Utils.Converters
{
    [ValueConversion(typeof(object), typeof(IEnumerable))]
    internal class EnumValuesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Type type = value.GetType();
            if (type.IsEnum)
            {
                var comparer = new EnumNameEqualityComparer(type);
                return Enum.GetValues(type).OfType<object>().Distinct(comparer).OrderBy(cur => Enum.GetName(type, cur)).ToList();
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private class EnumNameEqualityComparer : IEqualityComparer<object>
        {
            private Type _EnumType;

            public EnumNameEqualityComparer(Type enumType)
            {
                _EnumType = enumType;
            }

            public new bool Equals(object x, object y)
            {
                return Enum.GetName(_EnumType, x) == Enum.GetName(_EnumType, y);
            }

            public int GetHashCode(object obj)
            {
                return Enum.GetName(_EnumType, obj).GetHashCode();
            }
        }
    }
}
