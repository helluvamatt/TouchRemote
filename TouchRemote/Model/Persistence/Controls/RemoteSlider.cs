using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Serialization;
using TouchRemote.Lib;
using TouchRemote.Utils;
using TouchRemote.Web.Models;

namespace TouchRemote.Model.Persistence.Controls
{
    [Serializable]
    [XmlType(Namespace = "http://schneenet.com/Controls.xsd")]
    public sealed class RemoteSlider : RemoteElement
    {
        #region Dependency properties

        private string _Label;
        [Category("Appearance")]
        [DisplayName("Label")]
        [Description("Slider label")]
        [XmlAttribute]
        public string Label
        {
            get
            {
                return _Label;
            }
            set
            {
                ChangeAndNotify(ref _Label, value, () => Label);
            }
        }

        private Orientation _Orientation;
        [Category("Appearance")]
        [DisplayName("Orientation")]
        [Description("Slider orientation")]
        [XmlAttribute]
        public Orientation Orientation
        {
            get
            {
                return _Orientation;
            }
            set
            {
                ChangeAndNotify(ref _Orientation, value, () => Orientation);
            }
        }

        private FloatBoundPropertyInstance _Value;
        [Category("Behavior")]
        [DisplayName("Bound Value")]
        [Description("Bind the slider's value to this property")]
        [XmlElement("RemoteSlider.Value")]
        public FloatBoundPropertyInstance Value
        {
            get
            {
                return _Value;
            }
            set
            {
                ChangeAndNotifyDependency(ref _Value, value, () => Value, OnValueChanged);
            }
        }

        private void OnValueChanged(object sender, PropertyChangedEventArgs args)
        {
            Notify(() => Value);
        }

        private float _NormalizedValue;
        [Category("Data")]
        [DisplayName("Current Value")]
        [Description("Current value of the bound property and slider")]
        [ReadOnly(true)]
        [XmlIgnore]
        public float NormalizedValue
        {
            get
            {
                return _NormalizedValue;
            }
            set
            {
                ChangeAndNotify(ref _NormalizedValue, value, () => NormalizedValue);
            }
        }

        #endregion

        public RemoteSlider()
        {
            Value = new FloatBoundPropertyInstance();
        }

        [XmlIgnore]
        public override Dictionary<string, string> WebProperties
        {
            get
            {
                return new Dictionary<string, string>
                {
                    { "Value", NormalizedValue.ToString() },
                    { "Orientation", Orientation.ToString().ToLower() },
                    { "Label", Label },
                };
            }
        }

        [XmlIgnore]
        public override IEnumerable<string> BoundPropertyNames => new string[] { "Value" };

        [XmlIgnore]
        public override WebControl.WebControlType WebControlType => WebControl.WebControlType.Slider;

        public override bool CanHandleEvent(string eventName)
        {
            return eventName == "value-changed";
        }

        public override void ProcessEvent(string eventName, object eventData)
        {
            if (eventName == "value-changed")
            {
                float value = Convert.ToSingle(eventData);
                OnFloatBoundPropertyValueChanged("Value", value);
                if (Value.Descriptor != null && Value.Impl != null)
                {
                    Task.Factory.StartNew(() => {
                        Value.Impl.SetValue(value);
                    });
                }
            }
        }

        public override FloatBoundProperty GetFloatBoundProperty(string propertyName)
        {
            if (propertyName == "Value")
            {
                return Value.Impl;
            }
            return null;
        }

        public override void OnFloatBoundPropertyValueChanged(string propertyName, float value)
        {
            if (propertyName == "Value")
            {
                if (Value.Descriptor != null && Value.Impl != null)
                {
                    NormalizedValue = value;
                }
            }
        }

        internal override InflateResult Inflate(PluginManager pluginManager)
        {
            return InflateResult.Default
                .Append("For property \"RemoteSlider.Value\": ", Value.Inflate(pluginManager));
        }

        internal override void Deflate(XmlDocument document)
        {
            Value.Deflate(document);
        }
    }
}
