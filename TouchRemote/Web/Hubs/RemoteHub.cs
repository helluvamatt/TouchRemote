using JWT;
using JWT.Serializers;
using log4net;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections.Generic;
using TouchRemote.Model;
using TouchRemote.Web.Models;
using System.Threading.Tasks;
using System.Net;

namespace TouchRemote.Web.Hubs
{
    [HubName("remoteHub")]
    public class RemoteHub : Hub<IClient>
    {
        private readonly IRemoteControlService _RemoteControlService;
        private readonly IWebServer _WebServer;
        private readonly IJwtDecoder _JwtDecoder;
        private readonly ILog _Log;

        public RemoteHub(IRemoteControlService remoteControlService, IWebServer webServer)
        {
            _RemoteControlService = remoteControlService;
            _WebServer = webServer;
            var jsonSerializer = new JsonNetSerializer();
            _JwtDecoder = new JwtDecoder(jsonSerializer, new JwtValidator(jsonSerializer, new UtcDateTimeProvider()), new JwtBase64UrlEncoder());
            _Log = LogManager.GetLogger(GetType());
        }

        public override Task OnConnected()
        {
            AddConnection();
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            _WebServer.UnregisterConnection(Context.ConnectionId);
            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            AddConnection();
            return base.OnReconnected();
        }

        public AuthResponse<IEnumerable<WebControl>> GetControls(string token)
        {
            return Authenticate(token, () => _RemoteControlService.ControlList);
        }

        public AuthResponse<bool> ClickButton(string token, string id)
        {
            return Authenticate(token, () =>
            {
                Guid guid;
                if (Guid.TryParse(id, out guid))
                {
                    return _RemoteControlService.Click(guid);
                }
                return false;
            });
        }

        private AuthResponse<T> Authenticate<T>(string token, Func<T> callback)
        {
            string requiredPassword = _RemoteControlService.GetRequiredPassword();
            if (string.IsNullOrEmpty(requiredPassword))
            {
                return AuthResponse<T>.Authenticated(callback.Invoke());
            }
            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    var payload = _JwtDecoder.DecodeToObject<TokenPayload>(token);
                    if (payload.Key == requiredPassword)
                    {
                        return AuthResponse<T>.Authenticated(callback.Invoke());
                    }
                }
                catch (SignatureVerificationException)
                {
                    // Fall through
                }
                catch (Exception ex)
                {
                    _Log.Error(string.Format("Exception processing SignalR request authentication: {0}", ex.Message), ex);
                }
            }
            return AuthResponse<T>.NotAuthenticated;
        }

        private string GetString(string key)
        {
            object tempObject;
            Context.Request.Environment.TryGetValue(key, out tempObject);
            return tempObject as string;
        }

        private void AddConnection()
        {
            IPAddress remoteAddress = IPAddress.Parse(GetString("server.RemoteIpAddress"));
            IPAddress localAddress = IPAddress.Parse(GetString("server.LocalIpAddress"));
            int remotePort = int.Parse(GetString("server.RemotePort"));
            int localPort = int.Parse(GetString("server.LocalPort"));
            IPEndPoint remoteEndpoint = new IPEndPoint(remoteAddress, remotePort);
            IPEndPoint localEndpoint = new IPEndPoint(localAddress, localPort);
            Connection conn = new Connection
            {
                Id = Context.ConnectionId,
                RemoteEndpoint = remoteEndpoint,
                LocalEndpoint = localEndpoint
            };
            _WebServer.RegisterConnection(conn);
        }
    }

    public interface IClient
    {
        void UpdateControl(WebControl webButton);
        void RefreshControls();
    }
}
