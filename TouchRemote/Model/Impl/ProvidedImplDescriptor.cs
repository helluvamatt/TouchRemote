using TouchRemote.Lib;

namespace TouchRemote.Model.Impl
{
    internal class ProvidedImplDescriptor : ImplDescriptor
    {
        public ProvidedImplDescriptor(PluginManager pluginManager, PluginDescriptor plugin, IProvided provided) : base(pluginManager, plugin, provided)
        {
            Instance = provided;
        }

        public IProvided Instance { get; private set; }

        public override bool Equals(object obj)
        {
            var other = obj as ProvidedImplDescriptor;
            if (other != null)
            {
                return other.Instance.Equals(Instance);
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Instance.GetHashCode();
        }
    }
}
