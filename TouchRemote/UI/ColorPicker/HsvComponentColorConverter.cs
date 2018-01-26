using Spectrum;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace TouchRemote.UI.ColorPicker
{
    internal class HsvComponentColorConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 3)
            {
                double h = (double)values[0];
                double s = (double)values[1];
                double v = (double)values[2];
                var hsv = new Color.HSV(h, s, v);
                return hsv.ToWpfColor();
            }
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
