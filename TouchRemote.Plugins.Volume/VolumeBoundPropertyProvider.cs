using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchRemote.Lib;

namespace TouchRemote.Plugins.Volume
{
    public class VolumeBoundPropertyProvider : BoundPropertyProvider<VolumeBoundProperty, int>
    {
        public IEnumerable<VolumeBoundProperty> GetProperties()
        {
            // TODO
            throw new NotImplementedException();
        }
    }
}
