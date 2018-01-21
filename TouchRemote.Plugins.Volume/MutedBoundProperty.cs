using NAudio.CoreAudioApi;
using System.Linq;
using System.Xml;
using TouchRemote.Lib;

namespace TouchRemote.Plugins.Volume
{
    public class MutedBoundProperty : ProvidedBooleanBoundProperty
    {
        private const string NAME = "Mute - {0}";
        private const string DESCRIPTION = "Mute the volume for \"{0}\"";

        private string _DeviceId;
        private string _DeviceName;
        private MMDevice _Device;
        private bool _Muted;

        internal MutedBoundProperty(MMDevice device)
        {
            _Device = device;
            _DeviceId = device.ID;
            _DeviceName = device.DeviceFriendlyName;
        }

        public bool SupportsValueChanged => true;

        public string Name => _Device != null ? string.Format(NAME, _DeviceName) : "Mute";

        public string Description => _Device != null ? string.Format(DESCRIPTION, _DeviceName) : "";

        public event BoundPropertyValueChangedHandler<bool> ValueChanged;

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

        public void SetValue(bool parameter)
        {
            if (_Device != null)
            {
                _Device.AudioEndpointVolume.Mute = parameter;
            }
        }

        public bool GetValue()
        {
            if (_Device != null)
            {
                return _Device.AudioEndpointVolume.Mute;
            }
            return false;
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
            var other = obj as MutedBoundProperty;
            return other != null && other._DeviceId == _DeviceId;
        }

        public override int GetHashCode()
        {
            return _DeviceId.GetHashCode();
        }

        private void AudioEndpointVolume_OnVolumeNotification(AudioVolumeNotificationData data)
        {
            bool newValue = data.Muted;
            if (newValue != _Muted)
            {
                ValueChanged?.Invoke(this, new BoundPropertyValueChangedEventArgs<bool>(_Muted, newValue));
                _Muted = newValue;
            }
        }

        private MMDevice OpenDeviceWithId(string deviceId)
        {
            return new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active).FirstOrDefault(dev => dev.ID == deviceId);
        }
    }
}
