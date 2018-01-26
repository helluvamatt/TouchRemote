using System;
using System.Collections.ObjectModel;
using System.Reflection;
using TouchRemote.Lib.Attributes;

namespace TouchRemote.UI.ConfigEditor.Model
{
    internal class ConfigPropertyCollection : Collection<ConfigProperty>
    {
        public static ConfigPropertyCollection FromObject(object configObject)
        {
            var collection = new ConfigPropertyCollection();
            if (configObject != null)
            {
                Type type = configObject.GetType();
                foreach (var propInfo in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    var attr = propInfo.GetCustomAttribute<ConfigPropertyAttribute>();
                    if (attr != null)
                    {
                        collection.Add(ConfigProperty.Create(propInfo));
                    }
                }
            }
            return collection;
        }
    }
}
