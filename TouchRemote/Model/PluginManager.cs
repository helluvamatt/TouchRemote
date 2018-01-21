using System;
using System.Collections.Generic;
using System.ComponentModel;
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
            PluginsDirectory = pluginDirectory;
            _Plugins = new Dictionary<string, PluginDescriptor>();

            var registration = new RegistrationBuilder();
            registration
                .ForTypesMatching<ActionExecutable>(CheckType<ActionExecutable>)
                .SetCreationPolicy(CreationPolicy.NonShared)
                .Export<ActionExecutable>(builder => builder
                    .AddMetadata("Name", t => GetImplName(t))
                    .AddMetadata("Description", t => GetImplDescription(t))
                    .AddMetadata("Type", t => t)
                    .AddMetadata("Plugin", t => _Plugins[t.Assembly.FullName])
                    .AddMetadata("Manager", this));
            registration
                .ForTypesMatching<FloatBoundProperty>(CheckType<FloatBoundProperty>)
                .SetCreationPolicy(CreationPolicy.NonShared)
                .Export<FloatBoundProperty>(builder => builder
                    .AddMetadata("Name", t => GetImplName(t))
                    .AddMetadata("Description", t => GetImplDescription(t))
                    .AddMetadata("Type", t => t)
                    .AddMetadata("Plugin", t => _Plugins[t.Assembly.FullName])
                    .AddMetadata("Manager", this));
            registration
                .ForTypesMatching<BooleanBoundProperty>(CheckType<BooleanBoundProperty>)
                .SetCreationPolicy(CreationPolicy.NonShared)
                .Export<BooleanBoundProperty>(builder => builder
                    .AddMetadata("Name", t => GetImplName(t))
                    .AddMetadata("Description", t => GetImplDescription(t))
                    .AddMetadata("Type", t => t)
                    .AddMetadata("Plugin", t => _Plugins[t.Assembly.FullName])
                    .AddMetadata("Manager", this));
            registration
                .ForTypesMatching<StringBoundProperty>(CheckType<StringBoundProperty>)
                .SetCreationPolicy(CreationPolicy.NonShared)
                .Export<StringBoundProperty>(builder => builder
                    .AddMetadata("Name", t => GetImplName(t))
                    .AddMetadata("Description", t => GetImplDescription(t))
                    .AddMetadata("Type", t => t)
                    .AddMetadata("Plugin", t => _Plugins[t.Assembly.FullName])
                    .AddMetadata("Manager", this));
            registration
                .ForTypesDerivedFrom<ActionExecutableProvider>()
                .SetCreationPolicy(CreationPolicy.Shared)
                .Export<ActionExecutableProvider>();
            registration
                .ForTypesDerivedFrom<FloatBoundPropertyProvider>()
                .SetCreationPolicy(CreationPolicy.Shared)
                .Export<FloatBoundPropertyProvider>();
            registration
                .ForTypesDerivedFrom<BooleanBoundPropertyProvider>()
                .SetCreationPolicy(CreationPolicy.Shared)
                .Export<BooleanBoundPropertyProvider>();
            registration
                .ForTypesDerivedFrom<StringBoundPropertyProvider>()
                .SetCreationPolicy(CreationPolicy.Shared)
                .Export<StringBoundPropertyProvider>();

            var list = new List<AssemblyCatalog>();
            AddAssembly(list, Assembly.GetExecutingAssembly(), registration);
            Directory.CreateDirectory(pluginDirectory);
            foreach (string file in Directory.EnumerateFiles(pluginDirectory).Where(filename => Path.GetExtension(filename).ToLower() == ".dll"))
            {
                Assembly assembly = Assembly.LoadFile(file);
                AddAssembly(list, assembly, registration);
            }

            var compositeCatalog = new AggregateCatalog(list);
            _Container = new CompositionContainer(compositeCatalog);
            _Container.ComposeParts(this);
        }

        private bool CheckType<T>(Type t)
        {
            // Check that `t`: (in order from left to right)
            // 1. Implements T (our target interface type)
            // 2. Does NOT implement IProvided (these are managed outside of the container)
            // 3. Is NOT an `abstract class` (want to ignore base classes from plugins)
            // 4. Is NOT an `interface` (want to ignore interfaces from the library and plugins)
            return typeof(T).IsAssignableFrom(t) && !typeof(IProvided).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface;
        }

        private void AddAssembly(List<AssemblyCatalog> catalogList, Assembly assembly, RegistrationBuilder builder)
        {
            var plugin = PluginDescriptor.Create(assembly);
            _Plugins.Add(plugin.Assembly.FullName, plugin);
            catalogList.Add(new AssemblyCatalog(assembly, builder));
        }

        public string PluginsDirectory { get; private set; }

        public IEnumerable<PluginDescriptor> Plugins
        {
            get
            {
                return _Plugins.Values.AsEnumerable();
            }
        }

        public IEnumerable<IImplDescriptor> Actions
        {
            get
            {
                return GetImplDescriptors<ActionExecutable, ProvidedActionExecutable, ActionExecutableProvider>();
            }
        }

        public IEnumerable<IImplDescriptor> FloatBoundProperties
        {
            get
            {
                return GetImplDescriptors<FloatBoundProperty, ProvidedFloatBoundProperty, FloatBoundPropertyProvider>();
            }
        }

        public IEnumerable<IImplDescriptor> BooleanBoundProperties
        {
            get
            {
                return GetImplDescriptors<BooleanBoundProperty, ProvidedBooleanBoundProperty, BooleanBoundPropertyProvider>();
            }
        }

        public IEnumerable<IImplDescriptor> StringBoundProperties
        {
            get
            {
                return GetImplDescriptors<StringBoundProperty, ProvidedStringBoundProperty, StringBoundPropertyProvider>();
            }
        }

        public IImplDescriptor GetActionDescriptor(string typeName)
        {
            return GetImplDescriptor<ActionExecutable, ProvidedActionExecutable, ActionExecutableProvider>(typeName);
        }

        public IImplDescriptor GetFloatBoundPropertyDescriptor(string typeName)
        {
            return GetImplDescriptor<FloatBoundProperty, ProvidedFloatBoundProperty, FloatBoundPropertyProvider>(typeName);
        }

        public IImplDescriptor GetBooleanBoundPropertyDescriptor(string typeName)
        {
            return GetImplDescriptor<BooleanBoundProperty, ProvidedBooleanBoundProperty, BooleanBoundPropertyProvider>(typeName);
        }

        public IImplDescriptor GetStringBoundPropertyDescriptor(string typeName)
        {
            return GetImplDescriptor<StringBoundProperty, ProvidedStringBoundProperty, StringBoundPropertyProvider>(typeName);
        }

        public ActionExecutable GetActionInstance(IImplDescriptor descriptor)
        {
            return GetImplInstance<ActionExecutable, ProvidedActionExecutable, ActionExecutableProvider>(descriptor);
        }

        public FloatBoundProperty GetFloatBoundPropertyInstance(IImplDescriptor descriptor)
        {
            return GetImplInstance<FloatBoundProperty, ProvidedFloatBoundProperty, FloatBoundPropertyProvider>(descriptor);
        }

        public BooleanBoundProperty GetBooleanBoundPropertyInstance(IImplDescriptor descriptor)
        {
            return GetImplInstance<BooleanBoundProperty, ProvidedBooleanBoundProperty, BooleanBoundPropertyProvider>(descriptor);
        }

        public StringBoundProperty GetStringBoundPropertyInstance(IImplDescriptor descriptor)
        {
            return GetImplInstance<StringBoundProperty, ProvidedStringBoundProperty, StringBoundPropertyProvider>(descriptor);
        }

        private static string GetImplName(Type type)
        {
            string name = type.GetAttributeValue<DisplayNameAttribute, string>(attr => attr.DisplayName);
            if (string.IsNullOrEmpty(name)) name = type.Name;
            return name;
        }

        public static string GetImplDescription(Type type)
        {
            return type.GetAttributeValue<DescriptionAttribute, string>(attr => attr.Description);
        }

        private IEnumerable<IImplDescriptor> GetImplDescriptors<TImpl, TProvidedImpl, TProvider>() where TProvider : IProvider<TProvidedImpl> where TProvidedImpl : IProvided where TImpl : class
        {
            var impls = _Container.GetExports<TImpl, IImplDescriptor>().Select(export => new ImplDescriptor(export.Metadata)).ToList();
            foreach (var provider in _Container.GetExports<TProvider>().Select(export => export.Value))
            {
                var plugin = _Plugins[provider.GetType().Assembly.FullName];
                impls.AddRange(provider.GetProperties().Select(provided => new ProvidedImplDescriptor(this, plugin, provided)));
            }
            impls.Insert(0, null);
            return impls;
        }

        private IImplDescriptor GetImplDescriptor<TImpl, TProvidedImpl, TProvider>(string typeName) where TProvider : IProvider<TProvidedImpl> where TProvidedImpl : IProvided where TImpl : class
        {
            // typeName will be something like "PluginName.Actions.PluginCustomBoundPropertyName"
            try
            {
                foreach (var provider in _Container.GetExports<TProvider>().Select(export => export.Value))
                {
                    var plugin = _Plugins[provider.GetType().Assembly.FullName];
                    var instance = provider.EmptyInstance;
                    if (instance.GetType().FullName == typeName)
                    {
                        return new ProvidedImplDescriptor(this, plugin, instance);
                    }
                }
                var fromContainer = _Container.GetExports<TImpl, IImplDescriptor>().Select(export => export.Metadata).FirstOrDefault(metadata => metadata.Type.FullName.Equals(typeName));
                return fromContainer != null ? new ImplDescriptor(fromContainer) : null;
            }
            catch
            {
                return null;
            }
        }

        private TImpl GetImplInstance<TImpl, TProvidedImpl, TProvider>(IImplDescriptor descriptor) where TProvider : IProvider<TProvidedImpl> where TProvidedImpl : IProvided, TImpl where TImpl : class
        {
            var providing = descriptor as ProvidedImplDescriptor;
            if (providing != null)
            {
                return providing.Instance as TImpl;
            }
            return _Container.GetExports<TImpl, IImplDescriptor>().FirstOrDefault(entry => entry.Metadata.Type.FullName.Equals(descriptor.Type.FullName))?.Value;
        }
        

    }
}
