using System;
using System.Reflection;

namespace TouchRemote.UI.ConfigEditor.Model
{
    internal class ConfigProperty
    {
        private PropertyInfo _PropInfo;

        public string DisplayName { get; private set; }

        public string Description { get; private set; }

        public string Name => _PropInfo.Name;

        public Type Type => _PropInfo.PropertyType;

        public string Category { get; private set; }

        public int SortOrder { get; private set; }

        public object GetValue(object obj)
        {
            return _PropInfo.GetValue(obj);
        }

        public void SetValue(object obj, object value)
        {
            _PropInfo.SetValue(obj, value);
        }

        public ConfigProperty(PropertyInfo propInfo, string displayName, string description, string category, int order)
        {
            _PropInfo = propInfo;
            DisplayName = displayName;
            Description = description;
            Category = category;
            SortOrder = order;
        }
    }
}
