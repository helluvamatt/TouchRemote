using log4net;
using Nancy.Owin;
using Owin;
using SuaveWeb = Suave.Web;
using SuaveHttp = Suave.Http;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TouchRemote.Model;
using TouchRemote.Utils;
using TouchRemote.Web;
using Microsoft.FSharp.Control;
using Microsoft.FSharp.Core;
using Microsoft.FSharp.Collections;
using System.Threading;
using Microsoft.Owin.Builder;
using Microsoft.AspNet.SignalR;
using TouchRemote.Web.Hubs;

namespace TouchRemote
{
    internal class WebServer : DependencyObject, IWebServer, IDisposable
    {
        #region Dependency properties

        #region Status

        public static readonly DependencyProperty StatusProperty = DependencyProperty.Register("Status", typeof(ServerStatus), typeof(WebServer));
        public static readonly DependencyProperty ErrorProperty = DependencyProperty.Register("Error", typeof(Exception), typeof(WebServer));

        public ServerStatus Status
        {
            get
            {
                return (ServerStatus)GetValue(StatusProperty);
            }
            set
            {
                SetValue(StatusProperty, value);
            }
        }

        public Exception Error
        {
            get
            {
                return (Exception)GetValue(ErrorProperty);
            }
            set
            {
                SetValue(ErrorProperty, value);
            }
        }

        #endregion

        #region Listening Addresses

        public ObservableCollection<Uri> ListenAddresses { get; private set; }

        #endregion

        #region Clients

        public ObservableCollection<Connection> Clients { get; private set; }

        #endregion

        #endregion

        private ILog _Log;

        private RemoteControlService _RemoteControlService;

        private CancellationTokenSource _CancellationTokenSource;

        private IPEndPoint[] _ListenAddresses;

        public WebServer(RemoteControlService service, IPEndPoint[] listenAddresses)
        {
            Status = ServerStatus.Starting;
            _Log = LogManager.GetLogger(GetType());
            _RemoteControlService = service;
            _CancellationTokenSource = new CancellationTokenSource();
            // TODO Build optional SSL support with certificates
            string scheme = "http";
            ListenAddresses = new ObservableCollection<Uri>(listenAddresses.Select(ep => new UriBuilder(scheme, ep.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6 ? ("[" + ep.Address.ToString() + "]") : ep.Address.ToString(), ep.Port).Uri));
            _ListenAddresses = listenAddresses;
            Clients = new ObservableCollection<Connection>();
        }

        public void Start()
        {
            _Log.Info("WebServer is starting...");
            try
            {
                // TODO Build optional SSL support with certificates
                SuaveHttp.Protocol protocol = SuaveHttp.Protocol.HTTP;
                FSharpList<SuaveHttp.HttpBinding> bindings = ListModule.OfSeq(_ListenAddresses.Select(ep => new SuaveHttp.HttpBinding(protocol, new Suave.Sockets.SocketBinding(ep.Address, (ushort)ep.Port))));
                var suaveConfig = SuaveWeb.defaultConfig.withCancellationToken(_CancellationTokenSource.Token).withBindings(bindings);
                GlobalHost.DependencyResolver.Register(typeof(RemoteHub), () => new RemoteHub(_RemoteControlService, this));
                AppBootstrapper bootstrapper = new AppBootstrapper(_RemoteControlService);
                NancyOptions nancyOptions = new NancyOptions();
                nancyOptions.Bootstrapper = bootstrapper;
                AppBuilder appBuilder = new AppBuilder();
                appBuilder.MapSignalR();
                appBuilder.UseNancy(nancyOptions);
                var owin = appBuilder.Build();
                var app = Suave.Owin.OwinAppModule.OfAppFunc("", owin);
                var startAction = SuaveWeb.startWebServerAsync(suaveConfig, app);
                FSharpAsync.Start(startAction.Item2, FSharpOption<CancellationToken>.Some(_CancellationTokenSource.Token));
                FSharpAsync.StartAsTask(startAction.Item1, FSharpOption<TaskCreationOptions>.Some(TaskCreationOptions.None), FSharpOption<CancellationToken>.None).ContinueWith((started) => {
                    _Log.Info("WebServer is running.");
                    this.Invoke(() => Status = ServerStatus.Ready);
                });
            }
            catch (Exception ex)
            {
                _Log.Error(string.Format("Failed to start web server: {0}", ex.Message), ex);
                this.Invoke(() => Status = ServerStatus.Stopped);
            }
        }

        public void Dispose()
        {
            _CancellationTokenSource.Cancel();
        }

        public enum ServerStatus
        {
            Stopped, Starting, Ready
        }

        #region IWebServer implementation

        public void RegisterConnection(Connection connection)
        {
            this.Invoke(() => Clients.Add(connection));
        }

        public void UnregisterConnection(string connectionId)
        {
            this.Invoke(() => {
                var conn = Clients.FirstOrDefault(c => c.Id == connectionId);
                if (conn != null)
                {
                    Clients.Remove(conn);
                }
            });
        }

        public IEnumerable<Connection> Connections
        {
            get
            {
                return this.Invoke(() => {
                    return new List<Connection>(Clients);
                });
            }
        }

        #endregion
    }
}
