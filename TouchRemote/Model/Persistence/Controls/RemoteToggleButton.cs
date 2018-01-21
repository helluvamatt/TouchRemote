using System;
using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;

namespace TouchRemote.Model.Persistence.Controls
{
    [Serializable]
    [XmlType(Namespace = "http://schneenet.com/Controls.xsd")]
    public sealed class RemoteToggleButton : RemoteToggleButtonBase
    {
        #region Dependency properties

        private ActionExecutableInstance _ToggleOn;
        [Category("Behavior")]
        [DisplayName("Toggle On Action")]
        [Description("Action to perform when the button is toggled on.")]
        [XmlElement("RemoteToggleButton.ToggleOnAction")]
        public ActionExecutableInstance ToggleOn
        {
            get
            {
                return _ToggleOn;
            }
            set
            {
                ChangeAndNotifyDependency(ref _ToggleOn, value, () => ToggleOn, OnToggleOnChanged);
            }
        }

        private void OnToggleOnChanged(object sender, PropertyChangedEventArgs args)
        {
            Notify(() => ToggleOn);
        }

        private ActionExecutableInstance _ToggleOff;
        [Category("Behavior")]
        [DisplayName("Toggle Off Action")]
        [Description("Action to perform when the button is toggled off.")]
        [XmlElement("RemoteToggleButton.ToggleOffAction")]
        public ActionExecutableInstance ToggleOff
        {
            get
            {
                return _ToggleOff;
            }
            set
            {
                ChangeAndNotifyDependency(ref _ToggleOff, value, () => ToggleOff, OnToggleOffChanged);
            }
        }

        private void OnToggleOffChanged(object sender, PropertyChangedEventArgs args)
        {
            Notify(() => ToggleOff);
        }

        #endregion

        public RemoteToggleButton()
        {
            ToggleOn = new ActionExecutableInstance();
            ToggleOff = new ActionExecutableInstance();
        }

        protected override void HandleToggle()
        {
            if (Toggled)
            {
                if (ToggleOn.Descriptor != null && ToggleOn.Impl != null)
                {
                    ToggleOn.Impl.Execute();
                }
            }
            else
            {
                if (ToggleOff.Descriptor != null && ToggleOff.Impl != null)
                {
                    ToggleOff.Impl.Execute();
                }
            }
        }

        internal override InflateResult Inflate(PluginManager pluginManager)
        {
            return InflateResult.Default
                .Append("For property \"RemoteToggleButton.ToggleOn\": ", ToggleOn.Inflate(pluginManager))
                .Append("For property \"RemoteToggleButton.ToggleOff\": ", ToggleOff.Inflate(pluginManager));
        }

        internal override void Deflate(XmlDocument document)
        {
            ToggleOn.Deflate(document);
            ToggleOff.Deflate(document);
        }
    }
}
