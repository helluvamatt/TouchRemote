using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using TouchRemote.Lib.Attributes;

namespace TouchRemote.UI.ConfigEditor.Model
{
    internal class ConfigProperty : PropertyDescriptor
    {
        public const string DEFAULT_CATEGORY = "Parameters";

        private PropertyInfo _PropInfo;
        private bool _ReadOnly;

        public override Type ComponentType => _PropInfo.DeclaringType;

        public override bool IsReadOnly => _ReadOnly;

        public override Type PropertyType => _PropInfo.PropertyType;

        public override bool CanResetValue(object component)
        {
            return false;
        }

        public override object GetValue(object component)
        {
            return _PropInfo.GetValue(component);
        }

        public override void ResetValue(object component)
        {
            // No-op
        }

        public override void SetValue(object component, object value)
        {
            _PropInfo.SetValue(component, value);
        }

        public override bool ShouldSerializeValue(object component)
        {
            return true;
        }

        private ConfigProperty(PropertyInfo propInfo, bool readOnly, Attribute[] attrs) : base(propInfo.Name, attrs)
        {
            _PropInfo = propInfo;
            _ReadOnly = readOnly;
        }

        public static ConfigProperty Create(PropertyInfo propInfo)
        {
            var attrDisplayName = propInfo.GetCustomAttribute<DisplayNameAttribute>();
            var attrDescription = propInfo.GetCustomAttribute<DescriptionAttribute>();
            var attrCategory = propInfo.GetCustomAttribute<CategoryAttribute>();
            var attrOrder = propInfo.GetCustomAttribute<PropertyOrderAttribute>();

            List<Attribute> attrs = new List<Attribute>();

            if (attrDisplayName != null) attrs.Add(attrDisplayName);

            if (attrDescription != null) attrs.Add(attrDescription);

            if (attrCategory != null) attrs.Add(attrCategory);
            else attrs.Add(new CategoryAttribute(DEFAULT_CATEGORY));

            if (attrOrder != null) attrs.Add(new System.Windows.Controls.WpfPropertyGrid.PropertyOrderAttribute(attrOrder.Order));

            bool readOnly = false;
            var attrReadOnly = propInfo.GetCustomAttribute<ReadOnlyAttribute>();
            if (attrReadOnly != null) readOnly = attrReadOnly.IsReadOnly;

            return new ConfigProperty(propInfo, readOnly, attrs.ToArray());
        }
    }
}
