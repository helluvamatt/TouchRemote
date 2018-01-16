using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchRemote.Model;
using System.Reflection;
using System.IO;
using System.Net;
using System.Configuration;
using TouchRemote.Utils;
using TouchRemote.UI;
using log4net;
using System.ComponentModel;
using System.Windows;
using Hardcodet.Wpf.TaskbarNotification;
using R = TouchRemote.Properties.Resources;
using SDIcon = System.Drawing.Icon;
using TouchRemote.Properties;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Controls.Primitives;
using System.Threading;
using Mono.Options;
using System.Net.NetworkInformation;

namespace TouchRemote
{
    internal class AppContext : Application
    {
        #region Private members

        private string _AppDataPath;

        private WebServer _WebServer;

        private RemoteControlService _ControlService;
        private PluginManager _PluginManager;

        private TaskbarIcon _TaskbarIcon;

        private OptionsWindow _OptionsWindow;
        private bool _OnStartShowOptionsWindow = true;

        private Timer _CloseTimer;

        #endregion

        #region Properties

        public ILog Logger { get; private set; }

        #endregion

        public AppContext(string appData, string vendorName, string productName, string[] args)
        {
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            Logger = LogManager.GetLogger(GetType());
            _AppDataPath = appData;
            Directory.CreateDirectory(_AppDataPath);

            _CloseTimer = new Timer((state) => _TaskbarIcon.CloseBalloon());

            var settingsProvider = new RegistrySettingsProvider(vendorName, productName);
            Settings.Default.Providers.Add(settingsProvider);
            foreach (SettingsProperty prop in Settings.Default.Properties)
            {
                prop.Provider = settingsProvider;
            }
            Settings.Default.Reload();
            Settings.Default.PropertyChanged += (sender, e) => Settings.Default.Save();

            var opts = new OptionSet()
            {
                { "startup" , (opt) => _OnStartShowOptionsWindow = false }
            };
            opts.Parse(args);
        }

        public void InitializeContext()
        {
            int port = Settings.Default.ListenPort;
            List<IPEndPoint> endpoints = new List<IPEndPoint>();
            switch (Settings.Default.ListenAddressesMode)
            {
                case ListenAddressMode.LOCAL_ONLY:
                    endpoints.Add(new IPEndPoint(IPAddress.Loopback, port));
                    endpoints.Add(new IPEndPoint(IPAddress.IPv6Loopback, port));
                    break;
                case ListenAddressMode.CUSTOM:
                    if (Settings.Default.ListenAddressesCustom != null && Settings.Default.ListenAddressesCustom.Count > 0)
                    {
                        for (int i = Settings.Default.ListenAddressesCustom.Count - 1; i >= 0; i--)
                        {
                            var addrStr = Settings.Default.ListenAddressesCustom[i];
                            IPAddress addr;
                            if (IPAddress.TryParse(addrStr, out addr))
                            {
                                endpoints.Add(new IPEndPoint(addr, port));
                            }
                            else
                            {
                                Settings.Default.ListenAddressesCustom.RemoveAt(i);
                            }
                        }
                    }
                    else
                    {
                        Logger.Warn("Custom listen addresses was empty, falling back to localhost only...");
                        endpoints.Add(new IPEndPoint(IPAddress.Loopback, port));
                        endpoints.Add(new IPEndPoint(IPAddress.IPv6Loopback, port));
                    }
                    break;
                case ListenAddressMode.ANY:
                    foreach (NetworkInterface iface in NetworkInterface.GetAllNetworkInterfaces())
                    {
                        foreach (var ifaceAddr in iface.GetIPProperties().UnicastAddresses)
                        {
                            endpoints.Add(new IPEndPoint(ifaceAddr.Address, port));
                        }
                    }
                    break;
            }
            _PluginManager = new PluginManager(Path.Combine(Assembly.GetEntryAssembly().GetAssemblyDir(), "plugins"));
            _ControlService = new RemoteControlService(_AppDataPath, _PluginManager);
            _WebServer = new WebServer(_ControlService, endpoints.ToArray());
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            _TaskbarIcon = new TaskbarIcon();
            _TaskbarIcon.Icon = new SDIcon(GetType(), "appicon.ico");
            _TaskbarIcon.ToolTipText = R.AppTitle;
            _TaskbarIcon.DoubleClickCommand = new DelegateCommand(DoOpenOptions);
            _TaskbarIcon.TrayRightMouseUp += (sender, eventArgs) => DoToggleStatus();

            DoToggleStatus(() => _WebServer.Start());

            if (_OnStartShowOptionsWindow) DoOpenOptions();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Logger.Info("Shutting down...");
            _WebServer.Dispose();
            _TaskbarIcon.Dispose();
        }

        #region Event handlers

        private void DoToggleStatus(Action showCallback = null)
        {
            if (_TaskbarIcon.CustomBalloon != null && _TaskbarIcon.CustomBalloon.IsOpen)
            {
                _CloseTimer.Change(Timeout.Infinite, Timeout.Infinite);
                _TaskbarIcon.CloseBalloon();
            }
            else
            {
                var popup = new TrayIconPopup(_WebServer, () => _TaskbarIcon.CloseBalloon(), DoOpenOptions, Shutdown);
                popup.MouseEnter += (sender, eventArgs) => { _TaskbarIcon.ResetBalloonCloseTimer(); _CloseTimer.Change(Timeout.Infinite, Timeout.Infinite); };
                popup.MouseLeave += (sender, eventArgs) => _CloseTimer.Change(5000, Timeout.Infinite);
                popup.Loaded += (sender, eventArgs) => { showCallback?.Invoke(); showCallback = null; };
                _TaskbarIcon.ShowCustomBalloon(popup, PopupAnimation.Fade, 5000);
            }
        }

        private void DoOpenOptions()
        {
            if (_OptionsWindow == null)
            {
                _OptionsWindow = new OptionsWindow(_ControlService, _PluginManager, _WebServer, Shutdown);
                _OptionsWindow.Closed += (sender, e) => _OptionsWindow = null;
                _OptionsWindow.Show();
            }
            _OptionsWindow.Activate();
        }

        #endregion

        private Image GetImage(string resourceName, int width, int height)
        {
            var im = GetImage(resourceName);
            im.MaxWidth = width;
            im.MaxHeight = height;
            return im;
        }

        private Image GetImage(string resourceName)
        {
            return new Image { Source = new BitmapImage(new Uri(string.Format("pack://application:,,,/{0};component/{1}", Assembly.GetExecutingAssembly().GetName().Name, resourceName), UriKind.Absolute)) };
        }
    }
}
