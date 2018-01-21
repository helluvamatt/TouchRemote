using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchRemote.Lib.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class PluginIconAttribute : Attribute
    {
        public string IconClass { get; set; }

        public PluginIconAttribute(string iconClass)
        {
            IconClass = iconClass;
        }
    }
}
