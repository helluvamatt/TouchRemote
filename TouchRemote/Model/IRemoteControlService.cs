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

        IEnumerable<WebButton> ButtonList { get; }

        WebButton GetButton(Guid guid);

        string GetRequiredPassword();
    }
}
