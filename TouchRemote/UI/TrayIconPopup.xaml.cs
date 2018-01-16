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
using System.Windows.Navigation;
using System.Windows.Shapes;
using TouchRemote.Utils;

namespace TouchRemote.UI
{
    /// <summary>
    /// Interaction logic for TrayIconPopup.xaml
    /// </summary>
    internal partial class TrayIconPopup : UserControl
    {
        public ICommand CloseStatusPopupCommand { get; private set; }

        public ICommand OptionsFormCommand { get; private set; }

        public ICommand ShutdownCommand { get; private set; }

        public WebServer WebServer { get; private set; }

        public bool Hovered
        {
            get
            {
                return (bool)GetValue(HoveredProperty);
            }
            set
            {
                SetValue(HoveredProperty, value);
            }
        }

        public static readonly DependencyProperty HoveredProperty = DependencyProperty.Register("Hovered", typeof(bool), typeof(TrayIconPopup));

        public TrayIconPopup(WebServer webServer, Action closeStatusPopupCallback, Action optionsFormCallback, Action shutdownCallback)
        {
            WebServer = webServer;
            CloseStatusPopupCommand = new DelegateCommand(closeStatusPopupCallback);
            OptionsFormCommand = new DelegateCommand(optionsFormCallback);
            ShutdownCommand = new DelegateCommand(shutdownCallback);
            InitializeComponent();
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            Hovered = true;
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            Hovered = false;
        }
    }
}
