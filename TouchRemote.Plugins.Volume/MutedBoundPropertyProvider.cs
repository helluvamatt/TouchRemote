using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchRemote.Lib;

namespace TouchRemote.Plugins.Volume
{
    public class MutedBoundPropertyProvider : BooleanBoundPropertyProvider
    {
        public ProvidedBooleanBoundProperty EmptyInstance
        {
            get
            {
                var device = new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Render, Role.Console);
                return new MutedBoundProperty(device);
            }
        }

        public IEnumerable<ProvidedBooleanBoundProperty> GetProperties()
        {
            return new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active).Select(wasapi => new MutedBoundProperty(wasapi));
        }
    }
}
