using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using TouchRemote.Lib;

namespace TouchRemote.Model.Persistence.Controls
{
    public sealed class RemoteStringLabel : RemoteLabelBase
    {
        #region Dependency properties

        private StringBoundPropertyInstance _Value;
        [Category("Data")]
        [DisplayName("Bound Value")]
        [Description("Bind to this string bound property.")]
        [XmlElement("RemoteStringLabel.Value")]
        public StringBoundPropertyInstance Value
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
            Task.Run(() =>
            {
                string value = "";
                if (Value.Impl != null)
                {
                    value = Value.Impl.GetValue();
                }
                Text = value;
            });
        }

        #endregion

        public RemoteStringLabel()
        {
            Value = new StringBoundPropertyInstance();
        }

        #region Overrides

        [Browsable(false)]
        [XmlIgnore]
        public override IEnumerable<string> BoundPropertyNames => new string[] { "Value" };

        public override StringBoundProperty GetStringBoundProperty(string propertyName)
        {
            if (propertyName == "Value") return Value.Impl;
            return null;
        }

        public override void OnStringBoundPropertyValueChanged(string propertyName, string newValue)
        {
            if (propertyName == "Value")
            {
                Text = newValue;
            }
        }

        internal override void Deflate(XmlDocument document)
        {
            Value.Deflate(document);
        }

        internal override InflateResult Inflate(PluginManager pluginManager)
        {
            return InflateResult.Default
                .Append("For property \"RemoteStringLabel.Value\": ", Value.Inflate(pluginManager));
        }

        #endregion
    }
}
