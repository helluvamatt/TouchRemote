using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Xml.Serialization;
using TouchRemote.Lib;
using System.Xml;

namespace TouchRemote.Model.Persistence.Controls
{
    [Serializable]
    [XmlType(Namespace = "http://schneenet.com/Controls.xsd")]
    public sealed class RemoteBoundToggleButton : RemoteToggleButtonBase
    {
        #region Dependency properties

        private BooleanBoundPropertyInstance _ToggledProp;
        [Category("Behavior")]
        [DisplayName("Bound Value")]
        [Description("Bind the toggle button's toggle state to this property")]
        [XmlElement("RemoteBoundToggleButton.ToggledProp")]
        public BooleanBoundPropertyInstance ToggledProp
        {
            get
            {
                return _ToggledProp;
            }
            set
            {
                ChangeAndNotifyDependency(ref _ToggledProp, value, () => ToggledProp, OnToggledPropChanged);
            }
        }

        private void OnToggledPropChanged(object sender, PropertyChangedEventArgs args)
        {
            Notify(() => ToggledProp);
        }

        #endregion

        public RemoteBoundToggleButton()
        {
            ToggledProp = new BooleanBoundPropertyInstance();
        }

        [XmlIgnore]
        public override IEnumerable<string> BoundPropertyNames => new string[] { "ToggledProp" };

        public override BooleanBoundProperty GetBooleanBoundProperty(string propertyName)
        {
            if (propertyName == "ToggledProp")
            {
                return ToggledProp.Impl;
            }
            return null;
        }

        public override void OnBooleanBoundPropertyValueChanged(string propertyName, bool value)
        {
            if (propertyName == "ToggledProp")
            {
                if (ToggledProp.Descriptor != null && ToggledProp.Impl != null)
                {
                    Toggled = value;
                }
            }
        }

        protected override void HandleToggle()
        {
            if (ToggledProp.Descriptor != null && ToggledProp.Impl != null)
            {
                Task.Factory.StartNew(() => {
                    ToggledProp.Impl.SetValue(Toggled);
                });
            }
        }

        internal override InflateResult Inflate(PluginManager pluginManager)
        {
            return InflateResult.Default
                .Append("For property \"RemoteBoundToggleButton.ToggledProp\": ", ToggledProp.Inflate(pluginManager));
        }

        internal override void Deflate(XmlDocument document)
        {
            ToggledProp.Deflate(document);
        }
    }
}
