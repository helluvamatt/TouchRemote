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
    public sealed class RemoteBooleanLabel : RemoteLabelBase
    {
        #region Dependency properties

        private BooleanBoundPropertyInstance _Value;
        [Category("Data")]
        [DisplayName("Bound Value")]
        [Description("Bind to this boolean bound property.")]
        [XmlElement("RemoteBooleanLabel.Value")]
        public BooleanBoundPropertyInstance Value
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

        private string _TrueText;
        [Category("Data")]
        [DisplayName("True Text")]
        [Description("Text to show when the bound property is true")]
        [XmlAttribute]
        public string TrueText
        {
            get
            {
                return _TrueText;
            }
            set
            {
                ChangeAndNotify(ref _TrueText, value, () => TrueText, (oldValue, newValue) => UpdateLabelText());
            }
        }

        private string _FalseText;
        [Category("Data")]
        [DisplayName("False Text")]
        [Description("Text to show when the bound property is false")]
        [XmlAttribute]
        public string FalseText
        {
            get
            {
                return _FalseText;
            }
            set
            {
                ChangeAndNotify(ref _FalseText, value, () => FalseText, (oldValue, newValue) => UpdateLabelText());
            }
        }

        #endregion

        public RemoteBooleanLabel()
        {
            Value = new BooleanBoundPropertyInstance();
        }

        private void UpdateLabelText()
        {
            Task.Run(() =>
            {
                bool value = false;
                if (Value.Impl != null)
                {
                    value = Value.Impl.GetValue();
                }
                UpdateLabelText(value);
            });
        }

        private void UpdateLabelText(bool value)
        {
            Text = value ? TrueText : FalseText;
        }

        #region Overrides

        [XmlIgnore]
        [Browsable(false)]
        public override IEnumerable<string> BoundPropertyNames => new string[] { "Value" };

        public override BooleanBoundProperty GetBooleanBoundProperty(string propertyName)
        {
            if (propertyName == "Value") return Value.Impl;
            return null;
        }

        public override void OnBooleanBoundPropertyValueChanged(string propertyName, bool newValue)
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
                .Append("For property \"RemoteBooleanLabel.Value\": ", Value.Inflate(pluginManager));
        }

        #endregion
    }
}
