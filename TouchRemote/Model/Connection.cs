using System.Net;
using System.ComponentModel;
using TouchRemote.Utils;
using System;

namespace TouchRemote.Model
{
    public class Connection : INotifyPropertyChanged
    {
        public Connection(string id, IPEndPoint remoteEndpoint, IPEndPoint localEndpoint)
        {
            Id = id;
            RemoteEndpoint = remoteEndpoint;
            LocalEndpoint = localEndpoint;
        }

        public string Id { get; private set; }

        public IPEndPoint RemoteEndpoint { get; private set; }

        public IPEndPoint LocalEndpoint { get; private set; }

        private int _ClientWidth;
        public int ClientWidth
        {
            get
            {
                return _ClientWidth;
            }
            set
            {
                PropertyChanged.ChangeAndNotify(ref _ClientWidth, value, () => ClientWidth);
            }
        }

        private int _ClientHeight;
        public int ClientHeight
        {
            get
            {
                return _ClientHeight;
            }
            set
            {
                PropertyChanged.ChangeAndNotify(ref _ClientHeight, value, () => ClientHeight);
            }
        }

        private AuthState _AuthState;
        public AuthState AuthState
        {
            get
            {
                return _AuthState;
            }
            set
            {
                PropertyChanged.ChangeAndNotify(ref _AuthState, value, () => AuthState);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    [Flags]
    public enum AuthState
    {
        Authenticated = 0,
        NoPassword = 1,
        ExceedsMaxConnections = 2,
        IPNotAllowed = 4,
        InvalidConnection = 8,
    }
}
