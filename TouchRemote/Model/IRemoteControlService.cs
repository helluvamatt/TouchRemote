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
        bool Click(Guid guid);

        IEnumerable<WebControl> ControlList { get; }

        WebControl GetControl(Guid guid);

        string GetRequiredPassword();
    }
}
