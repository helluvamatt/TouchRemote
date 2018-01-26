using System.Collections.Generic;
using TouchRemote.Lib.Attributes;
using PropertyItem = System.Windows.Controls.WpfPropertyGrid.PropertyItem;

namespace TouchRemote.UI.ConfigEditor.Model
{
    internal class PropertyOrderComparer : IComparer<PropertyItem>
    {
        public int Compare(PropertyItem x, PropertyItem y)
        {
            var xAttr = x.GetAttribute<PropertyOrderAttribute>();
            var yAttr = y.GetAttribute<PropertyOrderAttribute>();
            int xVal = xAttr != null ? xAttr.Order : int.MaxValue;
            int yVal = yAttr != null ? yAttr.Order : int.MaxValue;
            return xVal.CompareTo(yVal);
        }
    }
}
