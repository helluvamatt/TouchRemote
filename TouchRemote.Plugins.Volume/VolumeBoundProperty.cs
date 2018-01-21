using NAudio.CoreAudioApi;
using System.Linq;
using System.Xml;
using TouchRemote.Lib;

namespace TouchRemote.Plugins.Volume
{
    public class VolumeBoundProperty : ProvidedFloatBoundProperty
    {
        private const string NAME = "Volume - {0}";
        private const string DESCRIPTION = "Adjust the volume for \"{0}\"";

        private string _DeviceId;
        private string _DeviceName;
        private MMDevice _Device;
        private float _Volume;

        public event BoundPropertyValueChangedHandler<float> ValueChanged;

        internal VolumeBoundProperty(MMDevice device)
        {
            _Device = device;
            _DeviceId = device.ID;
            _DeviceName = device.DeviceFriendlyName;
        }

        public string Name => _Device != null ? string.Format(NAME, _DeviceName) : "Volume";

        public string Description => _Device != null ? string.Format(DESCRIPTION, _DeviceName) : "";

        public bool SupportsValueChanged => true;

        public bool Initialize()
        {
            if (_Device == null) _Device = OpenDeviceWithId(_DeviceId);
            if (_Device != null)
            {
                _Device.AudioEndpointVolume.OnVolumeNotification += AudioEndpointVolume_OnVolumeNotification;
                return true;
            }
            return false;
        }

        public void SetValue(float parameter)
        {
            if (_Device != null)
            {
                _Device.AudioEndpointVolume.MasterVolumeLevelScalar = parameter;
            }
        }

        public float GetValue()
        {
            if (_Device != null)
            {
                return _Device.AudioEndpointVolume.MasterVolumeLevelScalar;
            }
            return 0f;
        }

        public void Dispose()
        {
            if (_Device != null)
            {
                _Device.AudioEndpointVolume.OnVolumeNotification -= AudioEndpointVolume_OnVolumeNotification;
                _Device.Dispose();
                _Device = null;
            }
        }

        public void Serialize(XmlElement element)
        {
            element.SetAttribute("Device", _DeviceId);
        }

        public void Deserialize(XmlElement element)
        {
            _DeviceId = element.GetAttribute("Device");
        }

        public override bool Equals(object obj)
        {
            var other = obj as VolumeBoundProperty;
            return other != null && other._DeviceId == _DeviceId;
        }

        public override int GetHashCode()
        {
            return _DeviceId.GetHashCode();
        }

        private void AudioEndpointVolume_OnVolumeNotification(AudioVolumeNotificationData data)
        {
            float newValue = data.MasterVolume;
            ValueChanged?.Invoke(this, new BoundPropertyValueChangedEventArgs<float>(_Volume, newValue));
            _Volume = newValue;
        }

        private MMDevice OpenDeviceWithId(string deviceId)
        {
            return new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active).FirstOrDefault(dev => dev.ID == deviceId);
        }
    }
}
