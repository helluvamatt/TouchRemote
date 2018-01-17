using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
                        var attrDisplayName = propInfo.GetCustomAttribute<DisplayNameAttribute>();
                        var attrDescription = propInfo.GetCustomAttribute<DescriptionAttribute>();
                        var attrCategory = propInfo.GetCustomAttribute<CategoryAttribute>();
                        var attrOrder = propInfo.GetCustomAttribute<PropertyOrderAttribute>();
                        collection.Add(new ConfigProperty(propInfo, attrDisplayName != null ? attrDisplayName.DisplayName : propInfo.Name, attrDescription != null ? attrDescription.Description : "", attrCategory != null ? attrCategory.Category : null, attrOrder != null ? attrOrder.Order : int.MaxValue));
                    }
                }
            }
            return collection;
        }

        private ConfigPropertyCollection() { }
    }
}
