using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TouchRemote.UI.ConfigEditor.Model
{
    internal class ConfigProperty : DependencyObject, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        #region Dependency properties

        public static readonly DependencyProperty DisplayNameProperty = DependencyProperty.Register("DisplayName", typeof(string), typeof(ConfigProperty));
        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register("Description", typeof(string), typeof(ConfigProperty));
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(object), typeof(ConfigProperty), new UIPropertyMetadata(ValueChanged));

        private static void ValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            (obj as ConfigProperty)?.PropertyChanged?.Invoke(obj, new PropertyChangedEventArgs(args.Property.Name));
        }

        public string DisplayName
        {
            get
            {
                return (string)GetValue(DisplayNameProperty);
            }
            private set
            {
                SetValue(DisplayNameProperty, value);
            }
        }

        public string Description
        {
            get
            {
                return (string)GetValue(DescriptionProperty);
            }
            private set
            {
                SetValue(DescriptionProperty, value);
            }
        }

        public object Value
        {
            get
            {
                return GetValue(ValueProperty);
            }
            set
            {
                SetValue(ValueProperty, value);
            }
        }

        #endregion

        public string Name { get; private set; }

        public Type Type { get; private set; }

        public ConfigProperty(string name, Type type, string displayName, string description, object value)
        {
            Name = name;
            Type = type;
            DisplayName = displayName;
            Description = description;
            Value = value;
        }
    }
}
