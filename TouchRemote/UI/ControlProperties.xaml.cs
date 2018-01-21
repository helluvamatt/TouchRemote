using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
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
using TouchRemote.Model;
using TouchRemote.Model.Persistence;
using TouchRemote.Model.Persistence.Controls;
using TouchRemote.UI.ConfigEditor;
using TouchRemote.UI.IconPicker;
using TouchRemote.Utils;
using Xceed.Wpf.Toolkit.PropertyGrid;

namespace TouchRemote.UI
{
    /// <summary>
    /// Interaction logic for ControlProperties.xaml
    /// </summary>
    internal partial class ControlProperties : MetroWindow
    {
        private IconManager _IconManager;

        #region Dependency properties

        public static readonly DependencyProperty SelectedObjectProperty = DependencyProperty.Register("SelectedObject", typeof(RemoteElement), typeof(ControlProperties));

        public RemoteElement SelectedObject
        {
            get
            {
                return (RemoteElement)GetValue(SelectedObjectProperty);
            }
            set
            {
                SetValue(SelectedObjectProperty, value);
            }
        }

        #endregion

        public PluginManager PluginManager { get; private set; }

        public ICommand ConfigCommand { get; private set; }

        public ICommand OpenIconPickerCommand { get; private set; }

        public ControlProperties(PluginManager pluginManager, IconManager iconManager, RemoteElement element)
        {
            PluginManager = pluginManager;
            _IconManager = iconManager;
            SelectedObject = element;
            ConfigCommand = new DelegateCommand<object>(DoConfig);
            OpenIconPickerCommand = new DelegateCommand<IconHolder>(OpenIconPicker);
            InitializeComponent();
        }

        private void DoConfig(object configObject)
        {
            EditorWindow window = new EditorWindow(configObject);
            window.Owner = this;
            window.ShowDialog();
        }

        private void OpenIconPicker(IconHolder iconHolder)
        {
            PickerWindow window = new PickerWindow(iconHolder, _IconManager);
            window.Owner = this;
            window.ShowDialog();
        }
    }
}
