using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchRemote.Lib.Attributes
{
    /// <summary>
    /// Manually specify the sort order for properties on implementations
    /// </summary>
    /// <remarks>
    /// <para>This attribute is applied to individual properties.</para>
    /// <para>It is recommended to also apply <code>System.ComponentModel.CategoryAttribute</code>. Otherwise all properties will be under a "Misc" category header in the UI.</para>
    /// <para>This attribute actually applies sort order within a category.</para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property)]
    public class PropertyOrderAttribute : Attribute
    {
        public int Order { get; set; }

        public PropertyOrderAttribute(int order)
        {
            Order = order;
        }
    }
}
