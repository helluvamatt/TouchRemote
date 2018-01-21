using log4net;
using Nancy.Owin;
using Owin;
using SuaveWeb = Suave.Web;
using SuaveHttp = Suave.Http;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
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
using TouchRemote.Model.Impl;
using TouchRemote.Properties;
using JWT;
using JWT.Algorithms;
using JWT.Serializers;
using TouchRemote.Web.Models;

namespace TouchRemote
{
    internal class WebServer : IWebServer, INotifyPropertyChanged, IDisposable
    {
        #region Dependency properties

        #region Status

        private ServerStatus _Status;
        public ServerStatus Status
        {
            get
            {
                return _Status;
            }
            set
            {
                PropertyChanged.ChangeAndNotify(ref _Status, value, () => Status);
            }
        }

        private Exception _Error;
        public Exception Error
        {
            get
            {
                return _Error;
            }
            set
            {
                PropertyChanged.ChangeAndNotify(ref _Error, value, () => Error);
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

        private readonly IJwtEncoder _JwtEncoder;
        private readonly IJwtDecoder _JwtDecoder;
        private readonly string _SecretKey;

        public event PropertyChangedEventHandler PropertyChanged;

        public WebServer(RemoteControlService service, IPEndPoint[] listenAddresses)
        {
            Status = ServerStatus.Starting;
            _Log = LogManager.GetLogger(GetType());
            _RemoteControlService = service;
            _CancellationTokenSource = new CancellationTokenSource();

            var jsonSerializer = new JsonNetSerializer();
            var base64Encoder = new JwtBase64UrlEncoder();
            _JwtEncoder = new JwtEncoder(new HMACSHA512Algorithm(), jsonSerializer, base64Encoder);
            _JwtDecoder = new JwtDecoder(jsonSerializer, new JwtValidator(jsonSerializer, new UtcDateTimeProvider()), base64Encoder);
            byte[] keyBytes = new byte[1024];
            new Random().NextBytes(keyBytes);
            _SecretKey = BitConverter.ToString(keyBytes).Replace("-", "");

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
                FSharpAsync.StartAsTask(startAction.Item1, FSharpOption<TaskCreationOptions>.Some(TaskCreationOptions.None), FSharpOption<CancellationToken>.None).ContinueWith((started) =>
                {
                    _Log.Info("WebServer is running.");
                    Status = ServerStatus.Ready;
                });
            }
            catch (Exception ex)
            {
                _Log.Error(string.Format("Failed to start web server: {0}", ex.Message), ex);
                Status = ServerStatus.Stopped;
                Error = ex;
            }
        }

        public void Dispose()
        {
            _CancellationTokenSource.Cancel();
        }

        private void InvokeAsync(Action callback)
        {
            if (!Application.Current.Dispatcher.HasShutdownStarted)
            {
                Application.Current.Dispatcher.InvokeAsync(callback);
            }
        }

        #region IWebServer implementation

        public void RegisterConnection(Connection connection)
        {
            if (Settings.Default.MaxSessions > 0 && Clients.Count + 1 > Settings.Default.MaxSessions)
            {
                connection.AuthState |= AuthState.ExceedsMaxConnections;
            }

            // TODO Set other authentication types, eg. IP address in whitelist (some parameters may need to be passed in from the hub context

            InvokeAsync(() => Clients.Add(connection));
        }

        public void UnregisterConnection(string connectionId)
        {
            var conn = Clients.FirstOrDefault(c => c.Id == connectionId);
            if (conn != null)
            {
                InvokeAsync(() => Clients.Remove(conn));
            }
        }

        public string CreateToken(string password)
        {
            return _JwtEncoder.Encode(TokenPayload.WithKey(password), _SecretKey);
        }

        public AuthState Login(string connectionId, string token)
        {
            var conn = Clients.FirstOrDefault(c => c.Id == connectionId);
            if (conn == null)
            {
                return AuthState.InvalidConnection;
            }

            AuthState authState = AuthState.Authenticated;

            // Check password
            string requiredPassword = Settings.Default.RequiredPassword;
            if (!string.IsNullOrEmpty(requiredPassword))
            {
                if (string.IsNullOrEmpty(token))
                {
                    authState |= AuthState.NoPassword;
                }
                else
                {
                    try
                    {
                        var payload = _JwtDecoder.DecodeToObject<TokenPayload>(token);
                        if (payload.Key != requiredPassword)
                        {
                            authState |= AuthState.NoPassword;
                        }
                    }
                    catch (SignatureVerificationException)
                    {
                        authState |= AuthState.NoPassword;
                    }
                }
            }

            // Check connection index against max connections
            if (Settings.Default.MaxSessions > 0 && Clients.IndexOf(conn) > Settings.Default.MaxSessions - 1)
            {
                authState |= AuthState.ExceedsMaxConnections;
            }

            // TODO Check other authentication types, eg. IP address in whitelist (some parameters may need to be passed in from the hub context

            return conn.AuthState = authState;
        }

        public AuthState CheckAuth(string connectionId, string token)
        {
            var conn = Clients.FirstOrDefault(c => c.Id == connectionId);
            if (conn == null) return AuthState.InvalidConnection;

            AuthState authState = AuthState.Authenticated;
            string requiredPassword = Settings.Default.RequiredPassword;
            if (!string.IsNullOrEmpty(requiredPassword))
            {
                if (string.IsNullOrEmpty(token))
                {
                    authState |= AuthState.NoPassword;
                }
                else
                {
                    try
                    {
                        var payload = _JwtDecoder.DecodeToObject<TokenPayload>(token);
                        if (payload.Key != requiredPassword)
                        {
                            authState |= AuthState.NoPassword;
                        }
                    }
                    catch (SignatureVerificationException)
                    {
                        authState |= AuthState.NoPassword;
                    }
                }
            }
            return authState | conn.AuthState;
        }

        public IEnumerable<Connection> Connections
        {
            get
            {
                return new List<Connection>(Clients);
            }
        }

        public void SetClientSize(string connectionId, int width, int height)
        {
            var conn = Clients.FirstOrDefault(c => c.Id == connectionId);
            if (conn != null)
            {
                conn.ClientWidth = width;
                conn.ClientHeight = height;
            }
        }

        #endregion

        public enum ServerStatus
        {
            Stopped, Starting, Ready
        }
    }
}
