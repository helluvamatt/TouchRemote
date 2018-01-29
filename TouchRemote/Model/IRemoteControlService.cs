using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchRemote.Web.Models;

namespace TouchRemote.Model
{
    public interface IRemoteControlService
    {
        bool ProcessEvent(Guid guid, string eventName, object eventData);

        IEnumerable<WebControl> ControlList { get; }
    }
}
