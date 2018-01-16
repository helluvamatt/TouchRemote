using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchRemote.Utils
{
    internal class RegistrySettingsProvider : SettingsProvider
    {
        public override string ApplicationName { get; set; }

        public string ApplicationVendorName { get; private set; }

        public override string Name => GetType().Name;

        public RegistrySettingsProvider(string vendor, string appName)
        {
            ApplicationVendorName = vendor;
            ApplicationName = appName;
        }

        public override SettingsPropertyValueCollection GetPropertyValues(SettingsContext context, SettingsPropertyCollection props)
        {
            SettingsPropertyValueCollection values = new SettingsPropertyValueCollection();
            using (RegistryKey key = OpenKey(false))
            {
                foreach (SettingsProperty setting in props)
                {
                    SettingsPropertyValue value = new SettingsPropertyValue(setting);
                    value.IsDirty = false;
                    value.SerializedValue = key.GetValue(setting.Name);
                    values.Add(value);
                }
            }
            return values;
        }

        public override void SetPropertyValues(SettingsContext context, SettingsPropertyValueCollection properties)
        {
            using (RegistryKey key = OpenKey(true))
            {
                foreach (SettingsPropertyValue value in properties)
                {
                    key.SetValue(value.Name, value.SerializedValue ?? "", RegistryValueKind.String);
                }
            }
        }

        private RegistryKey OpenKey(bool writable)
        {
            if (ApplicationName == null || ApplicationVendorName == null) throw new ArgumentNullException("ApplicationName or ApplicationVendorName were null.");
            return Registry.CurrentUser.CreateSubKey(string.Format("Software\\{0}\\{1}", ApplicationVendorName, ApplicationName), writable);
        }
    }
}
