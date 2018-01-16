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
    /// Interaction logic for ButtonProperties.xaml
    /// </summary>
    internal partial class ButtonProperties : MetroWindow
    {

        #region Dependency properties

        public static readonly DependencyProperty ButtonProperty = DependencyProperty.Register("Button", typeof(Model.Button), typeof(ButtonProperties));

        public Model.Button Button
        {
            get
            {
                return (Model.Button)GetValue(ButtonProperty);
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

        public ICommand ClickConfigCommand { get; private set; }

        public PluginManager PluginManager { get; private set; }

        #endregion

        public ButtonProperties(PluginManager pluginManager, Model.Button button)
        {
            PluginManager = pluginManager;
            ClickConfigCommand = new DelegateCommand(DoClickConfig);
            InitializeComponent();

            Button = button;
        }

        private void DoClickConfig()
        {
            ConfigEditor.EditorWindow editorWindow = new ConfigEditor.EditorWindow(Button.ClickActionImpl, () => Button.NotifyClickActionImplChanged());
            editorWindow.Owner = this;
            editorWindow.ShowDialog();
        }
    }
}
