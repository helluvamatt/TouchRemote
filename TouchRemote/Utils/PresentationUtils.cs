using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using TouchRemote.Model.Persistence.Controls;
using TouchRemote.Web.Models;

namespace TouchRemote.Utils
{
    internal static class PresentationUtils
    {
        public static T FindAncestor<T>(this DependencyObject obj) where T : DependencyObject
        {
            DependencyObject tmp = VisualTreeHelper.GetParent(obj);
            while (tmp != null && !(tmp is T))
            {
                tmp = VisualTreeHelper.GetParent(tmp);
            }
            return tmp as T;
        }

        public static WebControl ToWebControl(this RemoteElement e)
        {
            var style = new Dictionary<string, string>() {
                { "left", e.X.ToCssLength() },
                { "top", e.Y.ToCssLength() },
                { "zIndex", e.ZIndex.ToString() },
                { "width", e.Width.ToCssLength() },
                { "height", e.Height.ToCssLength() },
                { "color", e.Color.ToCssString() },
                { "backgroundColor", e.BackgroundColor.ToCssString() },
                { "textAlign", e.TextAlignment.ToString().ToLower() },
                { "justifyContent", e.TextAlignment.ToCssFlexbox() },
            };
            foreach (var kvp in e.ControlStyle)
            {
                style[kvp.Key] = kvp.Value;
            }
            return new WebControl(e.Id, e.WebControlType, style, e.WebProperties);
        }

        public static string ToCssLength(this double length)
        {
            // Double.NaN represents an auto sized control, otherwise, the length represents the size in pixels
            return double.IsNaN(length) || length < 0 ? "auto" : string.Format("{0}px", (int)Math.Round(length));
        }

        public static string ToHexString(this Color color)
        {
            return string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", color.A, color.R, color.G, color.B);
        }

        public static string ToCssString(this Color color)
        {
            return string.Format("rgba({0}, {1}, {2}, {3}", color.R, color.G, color.B, color.A / 255f);
        }

        public static string ToCssFlexbox(this TextAlignment alignment)
        {
            switch (alignment)
            {
                case TextAlignment.Left:
                    return "flex-start";
                case TextAlignment.Right:
                    return "flex-end";
                default:
                    return "center";
            }
        }
    }
}
