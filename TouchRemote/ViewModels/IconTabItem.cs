using FontAwesome.WPF;
using System.Windows;
using System.Windows.Controls;

namespace TouchRemote.ViewModels
{
    internal static class IconTabItem
    {
        public static void SetIcon(this TabItem tabItem, FontAwesomeIcon icon)
        {
            tabItem.SetValue(IconProperty, icon);
        }

        public static FontAwesomeIcon GetIcon(this TabItem tabItem)
        {
            return (FontAwesomeIcon)tabItem.GetValue(IconProperty);
        }

        public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(FontAwesomeIcon), typeof(TabItem));
    }
}
