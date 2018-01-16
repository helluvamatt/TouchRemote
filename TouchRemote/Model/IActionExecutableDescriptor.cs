using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchRemote.Model
{
    public interface IActionExecutableDescriptor
    {
        string Name { get; }

        string Description { get; }

        Type Type { get; }

        PluginDescriptor Plugin { get; }

        PluginManager Manager { get; }
    }
}
