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
        #region Private members

        private Point contextMenuOpenedHere;

        private ContentPresenter movingItem;
        private Point startPoint;

        #endregion

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

        #region Public properties

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

        #endregion

        public OptionsWindow(RemoteControlService remoteControlService, PluginManager pluginManager, WebServer webServer, Action shutdownCallback)
        {
            RemoteControlService = remoteControlService;
            PluginManager = pluginManager;
            WebServer = webServer;
            Interfaces = new ObservableCollection<InterfaceModel>();
            ToggleErrorDetails = new DelegateCommand(() => ErrorDetailsVisible = !ErrorDetailsVisible);
            AddButtonCommand = new DelegateCommand(AddButton);
            AddToggleButtonCommand = new DelegateCommand(AddToggleButton);
            OpenPropertiesCommand = new DelegateCommand<Element>(ShowButtonProperties);
            RemoveButtonCommand = new DelegateCommand<Element>(RemoveElement);
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

        #region Event handlers

        private void AddButton()
        {
            var pt = contextMenuOpenedHere;
            if (pt == null) pt = new Point(0, 0);
            RemoteControlService.AddElement(new Model.Button { Id = RemoteControlService.CreateId(), Icon = FontAwesome.WPF.FontAwesomeIcon.Square, Label = "New Button", X = (int)Math.Round(pt.X), Y = (int)Math.Round(pt.Y) });
        }

        private void AddToggleButton()
        {
            var pt = contextMenuOpenedHere;
            if (pt == null) pt = new Point(0, 0);
            RemoteControlService.AddElement(new ToggleButton { Id = RemoteControlService.CreateId(), IconOff = FontAwesome.WPF.FontAwesomeIcon.ToggleOff, IconOn = FontAwesome.WPF.FontAwesomeIcon.ToggleOn, LabelOff = "New Toggle Button", LabelOn = "New Toggle Button", X = (int)Math.Round(pt.X), Y = (int)Math.Round(pt.Y) });
        }

        private void ShowButtonProperties(Element element)
        {
            Window properties;
            if (element is ToggleButton)
            {
                properties = new ToggleButtonProperties(PluginManager, element as ToggleButton);
            }
            else if (element is Model.Button)
            {
                properties = new ButtonProperties(PluginManager, element as Model.Button);
            }
            else
            {
                return;
            }
            properties.Owner = this;
            properties.ShowDialog();
        }

        private void RemoveElement(Element element)
        {
            RemoteControlService.RemoveElement(element);
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

        private void ItemsControl_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var itemsControl = sender as ItemsControl;
            contextMenuOpenedHere = Mouse.GetPosition(itemsControl);
        }

        private void item_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = sender as ContentPresenter;
            var canvas = item.FindAncestor<Canvas>();

            startPoint = e.GetPosition(item);

            movingItem = item;

            // Obliterate Z-index for all children so the dragging item is on top
            foreach (UIElement child in canvas.Children)
            {
                Panel.SetZIndex(child, 1);
            }
            Panel.SetZIndex(item, 2);

            Mouse.Capture(item);
        }

        private void item_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (movingItem != null)
            {
                var item = sender as ContentPresenter;
                var canvas = item.FindAncestor<Canvas>();

                double newLeft = e.GetPosition(canvas).X - startPoint.X - canvas.Margin.Left;
                // newLeft inside canvas right-border?
                if (newLeft > canvas.Margin.Left + canvas.ActualWidth - item.ActualWidth)
                    newLeft = canvas.Margin.Left + canvas.ActualWidth - item.ActualWidth;
                // newLeft inside canvas left-border?
                else if (newLeft < canvas.Margin.Left)
                    newLeft = canvas.Margin.Left;
                item.SetValue(Canvas.LeftProperty, newLeft);

                double newTop = e.GetPosition(canvas).Y - startPoint.Y - canvas.Margin.Top;
                // newTop inside canvas bottom-border?
                if (newTop > canvas.Margin.Top + canvas.ActualHeight - item.ActualHeight)
                    newTop = canvas.Margin.Top + canvas.ActualHeight - item.ActualHeight;
                // newTop inside canvas top-border?
                else if (newTop < canvas.Margin.Top)
                    newTop = canvas.Margin.Top;
                item.SetValue(Canvas.TopProperty, newTop);
            }
        }

        private void item_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var item = sender as ContentPresenter;
            var canvas = item.FindAncestor<Canvas>();

            movingItem = null;

            // Obliterate Z-index for all children so the dragging item is on top
            foreach (UIElement child in canvas.Children)
            {
                Panel.SetZIndex(child, 1);
            }
            Panel.SetZIndex(item, 2);

            Mouse.Capture(null);
        }

        #endregion

        #region Model classes

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

        #endregion
    }
}
