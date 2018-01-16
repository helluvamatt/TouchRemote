using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchRemote.Web.Models
{
    internal class TokenPayload
    {
        public string Key { get; set; }

        public static TokenPayload WithKey(string password)
        {
            return new TokenPayload { Key = password };
        }
    }
}
