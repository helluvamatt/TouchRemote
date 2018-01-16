using System;

namespace TouchRemote.Model.Impl
{
    internal class ActionExecutableDescriptor : IActionExecutableDescriptor
    {
        public ActionExecutableDescriptor(IActionExecutableDescriptor other)
        {
            Name = other.Name;
            Description = other.Description;
            Type = other.Type;
            Plugin = other.Plugin;
            Manager = other.Manager;
        }

        public string Name { get; private set; }

        public string Description { get; private set; }

        public Type Type { get; private set; }

        public PluginDescriptor Plugin { get; private set; }

        public PluginManager Manager { get; private set; }

        public override bool Equals(object obj)
        {
            var other = obj as ActionExecutableDescriptor;
            return other != null && other.Type.FullName == Type.FullName;
        }

        public override int GetHashCode()
        {
            return Type.FullName.GetHashCode();
        }
    }
}
