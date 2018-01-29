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
        private const string GROUP_AUTHED = "authenticated";

        private readonly IRemoteControlService _RemoteControlService;
        private readonly IWebServer _WebServer;

        public RemoteHub(IRemoteControlService remoteControlService, IWebServer webServer)
        {
            _RemoteControlService = remoteControlService;
            _WebServer = webServer;
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

        public AuthResponse<string> Login(string password)
        {
            string token = _WebServer.CreateToken(password);
            AuthState authState = _WebServer.Login(Context.ConnectionId, token);
            if (authState == AuthState.Authenticated)
            {
                Groups.Add(Context.ConnectionId, GROUP_AUTHED);
                return AuthResponse<string>.Authenticated(token);
            }
            Groups.Remove(Context.ConnectionId, GROUP_AUTHED);
            return AuthResponse<string>.NotAuthenticated(authState);
        }

        public AuthResponse<IEnumerable<WebControl>> GetControls(string token)
        {
            return Authenticate(token, () => _RemoteControlService.ControlList);
        }

        public AuthResponse<bool> ProcessEvent(string token, string id, string eventName, object eventData)
        {
            return Authenticate(token, () =>
            {
                Guid guid;
                if (Guid.TryParse(id, out guid))
                {
                    return _RemoteControlService.ProcessEvent(guid, eventName, eventData);
                }
                return false;
            });
        }

        public AuthResponse SetClientSize(string token, int width, int height)
        {
            return Authenticate(token, () =>
            {
                _WebServer.SetClientSize(Context.ConnectionId, width, height);
            });
        }

        private AuthResponse<T> Authenticate<T>(string token, Func<T> callback)
        {
            AuthState authState = _WebServer.CheckAuth(Context.ConnectionId, token);
            if (authState == AuthState.Authenticated)
            {
                Groups.Add(Context.ConnectionId, GROUP_AUTHED);
                return AuthResponse<T>.Authenticated(callback.Invoke());
            }
            Groups.Remove(Context.ConnectionId, GROUP_AUTHED);
            return AuthResponse<T>.NotAuthenticated(authState);
        }

        private AuthResponse Authenticate(string token, Action callback)
        {
            AuthState authState = _WebServer.CheckAuth(Context.ConnectionId, token);
            if (authState == AuthState.Authenticated)
            {
                Groups.Add(Context.ConnectionId, GROUP_AUTHED);
                callback.Invoke();
                return AuthResponse.Authenticated;
            }
            Groups.Remove(Context.ConnectionId, GROUP_AUTHED);
            return AuthResponse.NotAuthenticated(authState);
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
            Connection conn = new Connection(Context.ConnectionId, remoteEndpoint, localEndpoint);
            _WebServer.RegisterConnection(conn);
        }

        public static IClient GetBroadcastContext()
        {
            return GlobalHost.ConnectionManager.GetHubContext<RemoteHub, IClient>().Clients.Group(GROUP_AUTHED);
        }
    }

    public interface IClient
    {
        void UpdateControl(WebControl webButton);
        void UpdateControlProperty(string id, string propertyName, string propertyValue);
        void RefreshControls();
    }
}
