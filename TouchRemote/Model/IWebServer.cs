using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchRemote.Model
{
    public interface IWebServer
    {
        void RegisterConnection(Connection connection);

        void UnregisterConnection(string connectionId);

        IEnumerable<Connection> Connections { get; }

        void SetClientSize(string connectionId, int width, int height);
    }

    public enum ListenAddressMode
    {
        LOCAL_ONLY, ANY, CUSTOM
    }
}
