using System.Collections.Generic;

namespace TouchRemote.Model
{
    public interface IWebServer
    {
        void RegisterConnection(Connection connection);

        void UnregisterConnection(string connectionId);

        string CreateToken(string password);

        AuthState Login(string connectionId, string token);

        AuthState CheckAuth(string connectionId, string token);

        IEnumerable<Connection> Connections { get; }

        void SetClientSize(string connectionId, int width, int height);
    }

    public enum ListenAddressMode
    {
        LOCAL_ONLY, ANY, CUSTOM
    }
}
