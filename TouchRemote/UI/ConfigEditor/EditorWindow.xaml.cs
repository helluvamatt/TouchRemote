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
using System.ComponentModel;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace TouchRemote.UI.ConfigEditor
{
    /// <summary>
    /// Interaction logic for ConfigurationEditorWindow.xaml
    /// </summary>
    internal partial class EditorWindow : MetroWindow
    {
        #region Dependency properties

        public static readonly DependencyProperty SelectedObjectProperty = DependencyProperty.Register("SelectedObject", typeof(object), typeof(EditorWindow));

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

        #endregion

        public EditorWindow(object action)
        {
            var configPropCollection = ConfigPropertyCollection.FromObject(action);
            InitializeComponent();
            foreach (var configProp in configPropCollection)
            {
                var def = new Xceed.Wpf.Toolkit.PropertyGrid.PropertyDefinition();
                def.TargetProperties = new string[] { configProp.Name };
                def.DisplayName = configProp.DisplayName;
                def.Description = configProp.Description;
                def.Category = configProp.Category;
                def.DisplayOrder = configProp.SortOrder;
                _PropertyGrid.PropertyDefinitions.Add(def);
            }
            SelectedObject = action;
        }
    }
}
