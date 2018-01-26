using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace TouchRemote.UI.ColorPicker
{
    internal class ComponentColorConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            byte r = 0, g = 0, b = 0, a = 255;
            if (values.Length > 2)
            {
                r = (byte)values[0];
                g = (byte)values[1];
                b = (byte)values[2];
                if (values.Length > 3)
                {
                    a = (byte)values[3];
                }
                return Color.FromArgb(a, r, g, b);
            }
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
