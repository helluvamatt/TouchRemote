using FontAwesome.WPF;
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
using TouchRemote.Utils;

namespace TouchRemote.UI
{
    /// <summary>
    /// Interaction logic for ToggleButtonProperties.xaml
    /// </summary>
    public partial class ToggleButtonProperties : MetroWindow
    {
        #region Dependency properties

        public static readonly DependencyProperty ButtonProperty = DependencyProperty.Register("Button", typeof(ToggleButton), typeof(ToggleButtonProperties));

        public ToggleButton Button
        {
            get
            {
                return (ToggleButton)GetValue(ButtonProperty);
            }
            set
            {
                SetValue(ButtonProperty, value);
            }
        }

        public IEnumerable<FontAwesomeIcon> Icons
        {
            get
            {
                var iconComparer = new IconComparer();
                return Enum.GetValues(typeof(FontAwesomeIcon))
                    .Cast<FontAwesomeIcon>()
                    .Distinct(iconComparer)
                    .OrderBy(icon => icon, iconComparer);
            }
        }

        public ICommand ToggleOnConfigCommand { get; private set; }

        public ICommand ToggleOffConfigCommand { get; private set; }

        public PluginManager PluginManager { get; private set; }

        #endregion

        public ToggleButtonProperties(PluginManager pluginManager, ToggleButton button)
        {
            PluginManager = pluginManager;
            ToggleOnConfigCommand = new DelegateCommand(DoToggleOnConfig);
            ToggleOffConfigCommand = new DelegateCommand(DoToggleOffConfig);
            InitializeComponent();
            Button = button;
        }

        private void DoToggleOnConfig()
        {
            ConfigEditor.EditorWindow editorWindow = new ConfigEditor.EditorWindow(Button.ToggleOnActionImpl, () => Button.NotifyToggleOnActionImplChanged());
            editorWindow.Owner = this;
            editorWindow.ShowDialog();
        }

        private void DoToggleOffConfig()
        {
            ConfigEditor.EditorWindow editorWindow = new ConfigEditor.EditorWindow(Button.ToggleOffActionImpl, () => Button.NotifyToggleOnActionImplChanged());
            editorWindow.Owner = this;
            editorWindow.ShowDialog();
        }
    }
}
