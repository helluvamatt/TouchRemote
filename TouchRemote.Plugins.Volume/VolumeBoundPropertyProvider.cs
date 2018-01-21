using NAudio.CoreAudioApi;
using System.Collections.Generic;
using System.Linq;
using TouchRemote.Lib;

namespace TouchRemote.Plugins.Volume
{
    public class VolumeBoundPropertyProvider : FloatBoundPropertyProvider
    {
        static VolumeBoundPropertyProvider()
        {
            CosturaUtility.Initialize();
        }

        public ProvidedFloatBoundProperty EmptyInstance
        {
            get
            {
                var device = new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Render, Role.Console);
                return new VolumeBoundProperty(device);
            }
        }

        public IEnumerable<ProvidedFloatBoundProperty> GetProperties()
        {
            return new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active).Select(wasapi => new VolumeBoundProperty(wasapi));
        }
    }
}
