using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
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
using TouchRemote.Properties;
using TouchRemote.Utils;
using R = TouchRemote.Properties.Resources;

namespace TouchRemote.UI
{
    /// <summary>
    /// Interaction logic for OptionsWindow.xaml
    /// </summary>
    internal partial class OptionsWindow : MetroWindow
    {
        #region Dependency properties

        public static readonly DependencyProperty PasswordRequiredProperty = DependencyProperty.Register("PasswordRequired", typeof(bool), typeof(OptionsWindow), new UIPropertyMetadata(PasswordRequiredChanged));
        public static readonly DependencyProperty SelectedPluginProperty = DependencyProperty.Register("SelectedPlugin", typeof(PluginDescriptor), typeof(OptionsWindow));
        public static readonly DependencyProperty ErrorDetailsVisibleProperty = DependencyProperty.Register("ErrorDetailsVisible", typeof(bool), typeof(OptionsWindow));

        private static void PasswordRequiredChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            bool passwordRequired = (bool)args.NewValue;
            if (passwordRequired)
            {
                if (string.IsNullOrEmpty(Settings.Default.RequiredPassword))
                {
                    byte[] pwBytes = new byte[8];
                    new Random().NextBytes(pwBytes);
                    Settings.Default.RequiredPassword = BitConverter.ToString(pwBytes).Replace("-", "");
                }
            }
            else
            {
                Settings.Default.RequiredPassword = "";
            }
        }

        public PluginDescriptor SelectedPlugin
        {
            get
            {
                return (PluginDescriptor)GetValue(SelectedPluginProperty);
            }
            set
            {
                SetValue(SelectedPluginProperty, value);
            }
        }

        public bool ErrorDetailsVisible
        {
            get
            {
                return (bool)GetValue(ErrorDetailsVisibleProperty);
            }
            set
            {
                SetValue(ErrorDetailsVisibleProperty, value);
            }
        }

        #endregion

        public ObservableCollection<InterfaceModel> Interfaces { get; private set; }

        public RemoteControlService RemoteControlService { get; private set; }

        public PluginManager PluginManager { get; private set; }

        public WebServer WebServer { get; private set; }

        public ICommand ToggleErrorDetails { get; private set; }

        public ICommand AddButtonCommand { get; private set; }

        public ICommand AddToggleButtonCommand { get; private set; }

        public ICommand OpenPropertiesCommand { get; private set; }

        public ICommand RemoveButtonCommand { get; private set; }

        public ICommand ShutdownCommand { get; private set; }

        public ICommand OpenUriCommand { get; private set; }

        public OptionsWindow(RemoteControlService remoteControlService, PluginManager pluginManager, WebServer webServer, Action shutdownCallback)
        {
            RemoteControlService = remoteControlService;
            PluginManager = pluginManager;
            WebServer = webServer;
            Interfaces = new ObservableCollection<InterfaceModel>();
            ToggleErrorDetails = new DelegateCommand(() => ErrorDetailsVisible = !ErrorDetailsVisible);
            AddButtonCommand = new DelegateCommand(AddButton);
            AddToggleButtonCommand = new DelegateCommand(AddToggleButton);
            OpenPropertiesCommand = new DelegateCommand<Model.Button>(ShowButtonProperties);
            RemoveButtonCommand = new DelegateCommand<Model.Button>(RemoveButton);
            ShutdownCommand = new DelegateCommand(shutdownCallback);
            OpenUriCommand = new DelegateCommand<Uri>(OpenUri);

            InitializeComponent();

            HashSet<IPAddress> addressSet = new HashSet<IPAddress>();
            if (Settings.Default.ListenAddressesCustom != null)
            {
                for (int i = Settings.Default.ListenAddressesCustom.Count - 1; i >= 0; i--)
                {
                    IPAddress addr;
                    if (!IPAddress.TryParse(Settings.Default.ListenAddressesCustom[i], out addr) || !addressSet.Add(addr))
                    {
                        Settings.Default.ListenAddressesCustom.RemoveAt(i);
                    }
                }
            }
            foreach (var iface in NetworkInterface.GetAllNetworkInterfaces())
            {
                string name = iface.Name;
                foreach (var ifaceAddressProp in iface.GetIPProperties().UnicastAddresses)
                {
                    Interfaces.Add(new InterfaceModel(name, ifaceAddressProp.Address, addressSet.Contains(ifaceAddressProp.Address), false, UpdateListenAddresses));
                    addressSet.Remove(ifaceAddressProp.Address);
                }
            }
            foreach (IPAddress addr in addressSet)
            {
                Interfaces.Add(new InterfaceModel(R.UnknownInterface, addr, true, true, UpdateListenAddresses));
            }

            SetValue(PasswordRequiredProperty, !string.IsNullOrEmpty(Settings.Default.RequiredPassword));
        }

        private void AddButton()
        {
            RemoteControlService.AddButton(new Model.Button { Id = RemoteControlService.CreateId(), Icon = FontAwesome.WPF.FontAwesomeIcon.Square, Label = "New Button" });
        }

        private void AddToggleButton()
        {
            RemoteControlService.AddButton(new ToggleButton { Id = RemoteControlService.CreateId(), IconOff = FontAwesome.WPF.FontAwesomeIcon.ToggleOff, IconOn = FontAwesome.WPF.FontAwesomeIcon.ToggleOn, LabelOff = "New Toggle Button", LabelOn = "New Toggle Button" });
        }

        private void ShowButtonProperties(Model.Button button)
        {
            Window properties;
            if (button is ToggleButton)
            {
                properties = new ToggleButtonProperties(PluginManager, button as ToggleButton);
            }
            else
            {
                properties = new ButtonProperties(PluginManager, button);
            }
            properties.Owner = this;
            properties.ShowDialog();
        }

        private void RemoveButton(Model.Button button)
        {
            RemoteControlService.RemoveButton(button);
        }

        private void UpdateListenAddresses(InterfaceModel model)
        {
            if (!model.Checked && model.UnknownInterface)
            {
                Interfaces.Remove(model);
            }

            StringCollection addressList = new StringCollection();
            addressList.AddRange(Interfaces.Where(m => m.Checked).Select(m => m.Address.ToString()).ToArray());
            Settings.Default.ListenAddressesCustom = addressList;
        }

        private void OpenUri(Uri uri)
        {
            try
            {
                Process.Start(uri.ToString());
            }
            catch (Exception)
            {
                // Ignore
            }
        }

        public class InterfaceModel
        {
            public string Name { get; private set; }
            public bool UnknownInterface { get; private set; }
            public IPAddress Address { get; private set; }
            public event Action<InterfaceModel> CheckedChanged;

            private bool _Checked;
            public bool Checked
            {
                get
                {
                    return _Checked;
                }
                set
                {
                    _Checked = value;
                    CheckedChanged?.Invoke(this);
                }
            }

            public InterfaceModel(string name, IPAddress addr, bool check, bool unknownInterface, Action<InterfaceModel> changeCallback)
            {
                Name = name;
                Address = addr;
                _Checked = check;
                UnknownInterface = unknownInterface;
                CheckedChanged += changeCallback;
            }

            public string Label
            {
                get
                {
                    return string.Format("{0} ({1})", Name, Address);
                }
            }
        }
    }
}
