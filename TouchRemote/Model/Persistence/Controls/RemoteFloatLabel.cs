using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using TouchRemote.Lib;

namespace TouchRemote.Model.Persistence.Controls
{
    [Serializable]
    [XmlType(Namespace = "http://schneenet.com/Controls.xsd")]
    public sealed class RemoteFloatLabel : RemoteLabelBase
    {
        #region Dependency properties

        private FloatBoundPropertyInstance _Value;
        [Category("Data")]
        [DisplayName("Bound Value")]
        [Description("Bind to this float bound property.")]
        [XmlElement("RemoteFloatLabel.Value")]
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
            UpdateLabelText();
        }

        private string _FormatString;
        [Category("Data")]
        [DisplayName("Format String")]
        [Description("Allows the use of a custom formatting string. Include \"{0}\" where the value should appear.")]
        [XmlAttribute]
        public string FormatString
        {
            get
            {
                return _FormatString;
            }
            set
            {
                ChangeAndNotify(ref _FormatString, value, () => FormatString, (oldValue, newValue) => UpdateLabelText());
            }
        }

        #endregion

        public RemoteFloatLabel()
        {
            Value = new FloatBoundPropertyInstance();
        }

        private void UpdateLabelText()
        {
            Task.Run(() =>
            {
                float newValue = 0;
                if (Value.Impl != null)
                {
                    newValue = Value.Impl.GetValue();
                }
                UpdateLabelText(newValue);
            });
        }

        private void UpdateLabelText(float value)
        {
            var format = FormatString;
            if (string.IsNullOrEmpty(format)) format = "{0}";
            try
            {
                Text = string.Format(format, value);
            }
            catch (FormatException ex)
            {
                Text = string.Format("#FormatError# {0} [{1}]", ex.Message, value);
            }
        }

        #region Overrides

        [XmlIgnore]
        public override IEnumerable<string> BoundPropertyNames => new string[] { "Value" };

        public override FloatBoundProperty GetFloatBoundProperty(string propertyName)
        {
            if (propertyName == "Value") return Value.Impl;
            return null;
        }

        public override void OnFloatBoundPropertyValueChanged(string propertyName, float newValue)
        {
            if (propertyName == "Value")
            {
                UpdateLabelText(newValue);
            }
        }

        internal override void Deflate(XmlDocument document)
        {
            Value.Deflate(document);
        }

        internal override InflateResult Inflate(PluginManager pluginManager)
        {
            return InflateResult.Default
                .Append("For property \"RemoteFloatLabel.Value\": ", Value.Inflate(pluginManager));
        }

        #endregion
    }
}
