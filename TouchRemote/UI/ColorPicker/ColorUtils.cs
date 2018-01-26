using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spectrum;
using WMColor = System.Windows.Media.Color;

namespace TouchRemote.UI.ColorPicker
{
    internal static class ColorUtils
    {
        public static Color.RGB ToRGB(this WMColor color)
        {
            return new Color.RGB(color.R, color.G, color.B);
        }

        public static Color.HSV ToHSV(this WMColor color)
        {
            return color.ToRGB().ToHSV();
        }

        public static WMColor ToWpfColor(this Color.RGB rgb)
        {
            return WMColor.FromRgb(rgb.R, rgb.G, rgb.B);
        }

        public static WMColor ToWpfColor(this Color.HSV hsv)
        {
            return hsv.ToRGB().ToWpfColor();
        }
    }
}
