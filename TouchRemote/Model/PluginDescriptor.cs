using System.Reflection;
using TouchRemote.Lib.Attributes;

namespace TouchRemote.Model
{
    public class PluginDescriptor
    {
        public static PluginDescriptor Create(Assembly assembly)
        {
            PluginDescriptor plugin = new PluginDescriptor();
            plugin.Assembly = assembly;
            plugin.Version = assembly.GetName().Version.ToString();
            var productNameAttr = assembly.GetCustomAttribute<AssemblyProductAttribute>();
            if (productNameAttr != null) plugin.Name = productNameAttr.Product;
            var productDescriptionAttr = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>();
            if (productDescriptionAttr != null) plugin.Description = productDescriptionAttr.Description;
            var iconAttr = assembly.GetCustomAttribute<PluginIconAttribute>();
            if (iconAttr != null) plugin.Icon = iconAttr.IconClass;
            return plugin;
        }

        private PluginDescriptor() { }

        public string Name { get; private set; }

        public string Description { get; private set; }

        public string Version { get; private set; }

        public string Icon { get; private set; }

        public Assembly Assembly { get; private set; }
    }
}
