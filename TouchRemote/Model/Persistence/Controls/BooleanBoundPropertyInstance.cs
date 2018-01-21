using System;
using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;
using TouchRemote.Lib;
using TouchRemote.Utils;

namespace TouchRemote.Model.Persistence.Controls
{
    [Serializable]
    [XmlType(Namespace = "http://schneenet.com/Controls.xsd")]
    public class BooleanBoundPropertyInstance : ImplInstance, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private IImplDescriptor _Descriptor;
        [XmlIgnore]
        public IImplDescriptor Descriptor
        {
            get
            {
                return _Descriptor;
            }
            set
            {
                PropertyChanged.ChangeAndNotify(ref _Descriptor, value, () => Descriptor, (oldValue, newValue) => Impl = newValue?.Manager.GetBooleanBoundPropertyInstance(newValue));
            }
        }

        private BooleanBoundProperty _Impl;
        [XmlIgnore]
        public BooleanBoundProperty Impl
        {
            get
            {
                return _Impl;
            }
            set
            {
                PropertyChanged.ChangeAndNotify(ref _Impl, value, () => Impl);
            }
        }

        internal override InflateResult Inflate(PluginManager pluginManager)
        {
            if (!string.IsNullOrEmpty(Type))
            {
                _Descriptor = pluginManager.GetBooleanBoundPropertyDescriptor(Type);
                if (_Descriptor != null)
                {
                    _Impl = pluginManager.GetBooleanBoundPropertyInstance(_Descriptor);
                    if (_Impl != null)
                    {
                        _Impl.Deserialize(Any);
                    }
                    else
                    {
                        return InflateResult.WithError(string.Format("Failed to find BooleanBoundProperty instance for type \"{0}\"", _Descriptor.Type.FullName));
                    }
                }
            }
            return InflateResult.Default;
        }

        internal override void Deflate(XmlDocument document)
        {
            if (_Descriptor != null && _Impl != null)
            {
                Type = _Descriptor.Type.FullName;
                Any = document.CreateElement(_Descriptor.Type.Name);
                _Impl.Serialize(Any);
            }
            else
            {
                Type = null;
                Any = null;
            }
        }
    }
}
