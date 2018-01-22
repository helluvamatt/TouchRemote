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
using TouchRemote.Model.Persistence.Controls;
using TouchRemote.Model.Impl;
using TouchRemote.Properties;
using TouchRemote.Utils;
using R = TouchRemote.Properties.Resources;
using FontAwesome.WPF;
using TouchRemote.Model.Persistence;
using System.ComponentModel;
using System.Collections;

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

        private IconManager _IconManager;

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

        public IPWhitelist IPWhitelist { get; private set; }

        public RemoteControlService RemoteControlService { get; private set; }

        public PluginManager PluginManager { get; private set; }

        public WebServer WebServer { get; private set; }

        public ICommand ToggleErrorDetails { get; private set; }

        public ICommand AddButtonCommand { get; private set; }

        public ICommand AddToggleButtonCommand { get; private set; }

        public ICommand AddBoundToggleButtonCommand { get; private set; }

        public ICommand AddSliderCommand { get; private set; }

        public ICommand OpenPropertiesCommand { get; private set; }

        public ICommand RemoveButtonCommand { get; private set; }

        public ICommand ShutdownCommand { get; private set; }

        public ICommand OpenUriCommand { get; private set; }

        public ICommand OpenPluginsDirectoryCommand { get; private set; }

        #endregion

        public OptionsWindow(RemoteControlService remoteControlService, PluginManager pluginManager, IconManager iconManager, WebServer webServer, Action shutdownCallback)
        {
            RemoteControlService = remoteControlService;
            PluginManager = pluginManager;
            _IconManager = iconManager;
            WebServer = webServer;
            IPWhitelist = new IPWhitelist();
            Interfaces = new ObservableCollection<InterfaceModel>();
            ToggleErrorDetails = new DelegateCommand(() => ErrorDetailsVisible = !ErrorDetailsVisible);
            AddButtonCommand = new DelegateCommand(AddButton);
            AddToggleButtonCommand = new DelegateCommand(AddToggleButton);
            AddBoundToggleButtonCommand = new DelegateCommand(AddBoundToggleButton);
            AddSliderCommand = new DelegateCommand(AddSlider);
            OpenPropertiesCommand = new DelegateCommand<RemoteElement>(ShowButtonProperties);
            RemoveButtonCommand = new DelegateCommand<RemoteElement>(RemoveElement);
            ShutdownCommand = new DelegateCommand(shutdownCallback);
            OpenUriCommand = new DelegateCommand<Uri>(OpenUri);
            OpenPluginsDirectoryCommand = new DelegateCommand(OpenPluginsDirectory);

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
            RemoteControlService.AddElement(new RemoteButton {
                Id = RemoteControlService.CreateId(),
                Label = "New Button",
                Icon = new IconHolder()
                {
                    Name = "Builtin: PlusSquareOutline",
                    Source = BuiltinIconSource.Create(FontAwesomeIcon.PlusSquareOutline, Colors.Black)
                },
                X = pt.X,
                Y = pt.Y,
                ZIndex = RemoteControlService.Count + 1
            });
        }

        private void AddToggleButton()
        {
            var pt = contextMenuOpenedHere;
            if (pt == null) pt = new Point(0, 0);
            RemoteControlService.AddElement(new RemoteToggleButton {
                Id = RemoteControlService.CreateId(),
                LabelOff = "New Toggle Button",
                LabelOn = "New Toggle Button",
                IconOff = new IconHolder()
                {
                    Name = "Builtin: ToggleOff",
                    Source = BuiltinIconSource.Create(FontAwesomeIcon.ToggleOff, Colors.Black)
                },
                IconOn = new IconHolder()
                {
                    Name = "Builtin: ToggleOn",
                    Source = BuiltinIconSource.Create(FontAwesomeIcon.ToggleOn, Colors.Black)
                },
                X = pt.X,
                Y = pt.Y,
                ZIndex = RemoteControlService.Count + 1
            });
        }

        private void AddBoundToggleButton()
        {
            var pt = contextMenuOpenedHere;
            if (pt == null) pt = new Point(0, 0);
            RemoteControlService.AddElement(new RemoteBoundToggleButton {
                Id = RemoteControlService.CreateId(),
                LabelOff = "New Bound Toggle Button",
                LabelOn = "New Bound Toggle Button",
                IconOff = new IconHolder()
                {
                    Name = "Builtin: ToggleOff",
                    Source = BuiltinIconSource.Create(FontAwesomeIcon.ToggleOff, Colors.Black)
                },
                IconOn = new IconHolder()
                {
                    Name = "Builtin: ToggleOn",
                    Source = BuiltinIconSource.Create(FontAwesomeIcon.ToggleOn, Colors.Black)
                },
                X = pt.X,
                Y = pt.Y,
                ZIndex = RemoteControlService.Count + 1
            });
        }

        private void AddSlider()
        {
            var pt = contextMenuOpenedHere;
            if (pt == null) pt = new Point(0, 0);
            RemoteControlService.AddElement(new RemoteSlider {
                Id = RemoteControlService.CreateId(),
                Label = "New Slider",
                X = pt.X,
                Y = pt.Y,
                ZIndex = RemoteControlService.Count + 1
            });
        }

        private void ShowButtonProperties(RemoteElement element)
        {
            Window properties = new ControlProperties(PluginManager, _IconManager, element);
            properties.Owner = this;
            properties.ShowDialog();
        }

        private void RemoveElement(RemoteElement element)
        {
            RemoteControlService.RemoveElement(element.Id);
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

        private void OpenPluginsDirectory()
        {
            try
            {
                Process.Start(PluginManager.PluginsDirectory);
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

            // Reset Z-index for all children so the dragging item is on top
            int itemZIndex = Panel.GetZIndex(item);
            foreach (UIElement child in canvas.Children)
            {
                int childZIndex = Panel.GetZIndex(child);
                if (childZIndex > itemZIndex) Panel.SetZIndex(child, childZIndex - 1);
            }
            Panel.SetZIndex(item, canvas.Children.Count);

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
            movingItem = null;
            Mouse.Capture(null);
        }

        #endregion
    }
}
