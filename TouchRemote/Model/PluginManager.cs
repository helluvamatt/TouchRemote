using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Registration;
using System.IO;
using System.Linq;
using System.Reflection;
using TouchRemote.Lib;
using TouchRemote.Model.Impl;
using TouchRemote.Utils;

namespace TouchRemote.Model
{
    public class PluginManager
    {
        private Dictionary<string, PluginDescriptor> _Plugins;
        private CompositionContainer _Container;

        public PluginManager(string pluginDirectory)
        {
            _Plugins = new Dictionary<string, PluginDescriptor>();

            var registration = new RegistrationBuilder();
            registration
                .ForTypesDerivedFrom<ActionExecutable>()
                .SetCreationPolicy(CreationPolicy.NonShared)
                .Export<ActionExecutable>(builder => builder
                    .AddMetadata("Name", t => GetActionName(t))
                    .AddMetadata("Description", t => GetActionDescription(t))
                    .AddMetadata("Type", t => t)
                    .AddMetadata("Plugin", t => _Plugins[t.Assembly.FullName])
                    .AddMetadata("Manager", this));

            var list = new List<AssemblyCatalog>();
            AddAssembly(list, Assembly.GetExecutingAssembly(), registration);
            Directory.CreateDirectory(pluginDirectory);
            foreach (string file in Directory.EnumerateFiles(pluginDirectory).Where(filename => Path.GetExtension(filename).Equals("dll")))
            {
                Assembly assembly = Assembly.LoadFile(file);
                AddAssembly(list, assembly, registration);
            }

            var compositeCatalog = new AggregateCatalog(list);
            _Container = new CompositionContainer(compositeCatalog);
            _Container.ComposeParts(this);
        }

        private void AddAssembly(List<AssemblyCatalog> catalogList, Assembly assembly, RegistrationBuilder builder)
        {
            var plugin = PluginDescriptor.Create(assembly);
            _Plugins.Add(plugin.Assembly.FullName, plugin);
            catalogList.Add(new AssemblyCatalog(assembly, builder));
        }

        public IEnumerable<PluginDescriptor> Plugins
        {
            get
            {
                return _Plugins.Values.AsEnumerable();
            }
        }

        public IEnumerable<IActionExecutableDescriptor> Actions
        {
            get
            {
                var actions = _Container.GetExports<ActionExecutable, IActionExecutableDescriptor>().Select(export => new ActionExecutableDescriptor(export.Metadata)).ToList();
                actions.Insert(0, null);
                return actions;
            }
        }

        public IActionExecutableDescriptor GetActionDescriptor(string typeName)
        {
            // typeName will be something like "PluginName.Actions.PluginCustomActionName"
            try
            {
                var fromContainer = _Container.GetExports<ActionExecutable, IActionExecutableDescriptor>().Select(export => export.Metadata).FirstOrDefault(metadata => metadata.Type.FullName.Equals(typeName));
                return fromContainer != null ? new ActionExecutableDescriptor(fromContainer) : null;
            }
            catch
            {
                return null;
            }
        }

        public ActionExecutable GetActionInstance(IActionExecutableDescriptor descriptor)
        {
            return _Container.GetExports<ActionExecutable, IActionExecutableDescriptor>().FirstOrDefault(entry => entry.Metadata.Type.FullName.Equals(descriptor.Type.FullName))?.Value;
        }

        private static string GetActionName(Type type)
        {
            string name = type.GetAttributeValue<ActionNameAttribute, string>(attr => attr.Name);
            if (string.IsNullOrEmpty(name)) name = type.Name;
            return name;
        }

        public static string GetActionDescription(Type type)
        {
            return type.GetAttributeValue<ActionDescriptionAttribute, string>(attr => attr.Description);
        }
    }
}
