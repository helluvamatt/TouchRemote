using System;
using System.Linq;
using MahApps.Metro.Controls;
using TouchRemote.UI.ConfigEditor.Model;
using System.Windows.Controls.WpfPropertyGrid;

namespace TouchRemote.UI.ConfigEditor
{
    /// <summary>
    /// Interaction logic for ConfigurationEditorWindow.xaml
    /// </summary>
    internal partial class EditorWindow : MetroWindow
    {
        public EditorWindow(object action)
        {
            var configPropCollection = ConfigPropertyCollection.FromObject(action);
            InitializeComponent();
            _PropertyGrid.SelectedObject = action;
            _PropertyGrid.Properties.Clear();
            foreach (var configProp in configPropCollection)
            {
                var item = new PropertyItem(_PropertyGrid, action, configProp);
                _PropertyGrid.Properties.Add(item);
            }
            foreach (var category in _PropertyGrid.Categories)
            {
                category.Comparer = new PropertyOrderComparer();
            }
        }
    }
}
