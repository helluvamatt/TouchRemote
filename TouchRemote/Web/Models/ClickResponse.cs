using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchRemote.Web.Models
{
    internal class ClickResponse
    {
        public ClickResponse()
        {
            Success = false;
            Button = null;
        }

        public bool Success { get; set; }

        public WebControl Button { get; set; }
    }
}
