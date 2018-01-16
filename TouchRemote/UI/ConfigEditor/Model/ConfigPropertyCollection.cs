using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using TouchRemote.Lib;

namespace TouchRemote.UI.ConfigEditor.Model
{
    internal class ConfigPropertyCollection : Collection<ConfigProperty>
    {
        public static ConfigPropertyCollection FromObject(object configObject, Action<ConfigProperty> changeCallback)
        {
            var collection = new ConfigPropertyCollection();
            if (configObject != null)
            {
                foreach (var propInfo in configObject.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    var attr = propInfo.GetCustomAttribute<ConfigPropertyAttribute>();
                    var attrDisplayName = propInfo.GetCustomAttribute<DisplayNameAttribute>();
                    var attrDescription = propInfo.GetCustomAttribute<DescriptionAttribute>();
                    if (attr != null && collection.CheckType(propInfo.PropertyType))
                    {
                        var configProp = new ConfigProperty(propInfo.Name, propInfo.PropertyType, attrDisplayName != null ? attrDisplayName.DisplayName : propInfo.Name, attrDescription != null ? attrDescription.Description : "", propInfo.GetValue(configObject));
                        configProp.PropertyChanged += (sender, args) => { propInfo.SetValue(configObject, configProp.Value); changeCallback?.Invoke(configProp); };
                        collection.Add(configProp);
                    }
                }
            }
            return collection;
        }

        private ConfigPropertyCollection() { }

        private bool CheckType(Type type)
        {
            return type.IsPrimitive
                || type.IsEnum
                || type.IsAssignableFrom(typeof(string))
                || type.IsAssignableFrom(typeof(DateTime))
                || type.IsAssignableFrom(typeof(TimeSpan))
                || type.IsAssignableFrom(typeof(Uri))
                || type.IsAssignableFrom(typeof(System.IO.FileInfo))
                || type.IsAssignableFrom(typeof(TimeZoneInfo))
                || (type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>)) && CheckType(Nullable.GetUnderlyingType(type)));
        }
    }
}
