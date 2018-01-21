using System;

namespace TouchRemote.Model
{
    public interface IImplDescriptor
    {
        string Name { get; }

        string Description { get; }

        Type Type { get; }

        PluginDescriptor Plugin { get; }

        PluginManager Manager { get; }
    }
}
