using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TouchRemote.Model
{
    public class Connection
    {
        public string Id { get; set; }

        public IPEndPoint RemoteEndpoint { get; set; }

        public IPEndPoint LocalEndpoint { get; set; }
    }
}
