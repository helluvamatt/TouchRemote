using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TouchRemote.Utils;
using TouchRemote.UI.ConfigEditor.Model;
using System.Reflection;
using TouchRemote.Lib;
using System.ComponentModel;

namespace TouchRemote.UI.ConfigEditor
{
    /// <summary>
    /// Interaction logic for ConfigurationEditorWindow.xaml
    /// </summary>
    internal partial class EditorWindow : MetroWindow
    {
        #region Dependency properties

        //public static readonly DependencyProperty SelectedPropertyProperty = DependencyProperty.Register("SelectedProperty", typeof(ConfigProperty), typeof(EditorWindow));
        public static readonly DependencyProperty SelectedObjectProperty = DependencyProperty.Register("SelectedObject", typeof(object), typeof(EditorWindow));

        //public ConfigPropertyCollection Properties { get; private set; }

        /*
        public ConfigProperty SelectedProperty
        {
            get
            {
                return (ConfigProperty)GetValue(SelectedPropertyProperty);
            }
            set
            {
                SetValue(SelectedPropertyProperty, value);
            }
        }
        */

        public object SelectedObject
        {
            get
            {
                return GetValue(SelectedObjectProperty);
            }
            set
            {
                SetValue(SelectedObjectProperty, value);
            }
        }

        private Action _PropertyChangeCallback;

        #endregion

        public EditorWindow(object action, Action propertyChangedCallback)
        {
            SelectedObject = action;
            _PropertyChangeCallback = propertyChangedCallback;
            //Properties = ConfigPropertyCollection.FromObject(action, changeCallback);
            InitializeComponent();
        }

        private void PropertyGrid_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            _PropertyChangeCallback.Invoke();
        }
    }
}
