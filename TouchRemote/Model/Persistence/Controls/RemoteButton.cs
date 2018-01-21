using FontAwesome.WPF;
using System.ComponentModel;
using TouchRemote.Lib;
using System.Collections.Generic;
using TouchRemote.Web.Models;
using TouchRemote.Utils;
using System.Xml.Serialization;
using System;
using System.Xml;

namespace TouchRemote.Model.Persistence.Controls
{
    [Serializable]
    [XmlType(Namespace = "http://schneenet.com/Controls.xsd")]
    public sealed class RemoteButton : RemoteButtonBase
    {
        #region Dependency properties

        [Category("Appearance")]
        [DisplayName("Icon")]
        [Description("Button icon")]
        [XmlElement("RemoteButton.Icon")]
        public override IconHolder Icon
        {
            get
            {
                return base.Icon;
            }
            set
            {
                base.Icon = value;
            }
        }

        [Category("Appearance")]
        [DisplayName("Label")]
        [Description("Button label text")]
        [XmlAttribute]
        public override string Label
        {
            get
            {
                return base.Label;
            }
            set
            {
                base.Label = value;
            }
        }

        private ActionExecutableInstance _Click;
        [Category("Behavior")]
        [DisplayName("Click Action")]
        [Description("Action to perform when the button is clicked.")]
        [XmlElement("RemoteButton.ClickAction")]
        public ActionExecutableInstance Click
        {
            get
            {
                return _Click;
            }
            set
            {
                ChangeAndNotifyDependency(ref _Click, value, () => Click, NotifyClickChanged);
            }
        }

        private void NotifyClickChanged(object sender, PropertyChangedEventArgs args)
        {
            Notify(() => Click);
        }

        #endregion

        public RemoteButton()
        {
            Click = new ActionExecutableInstance();
        }

        protected override void HandleClick()
        {
            if (Click.Descriptor != null && Click.Impl != null)
            {
                Click.Impl.Execute();
            }
        }

        internal override InflateResult Inflate(PluginManager pluginManager)
        {
            return InflateResult.Default
                .Append("For property \"RemoteButton.ClickAction\": ", _Click.Inflate(pluginManager));
        }

        internal override void Deflate(XmlDocument document)
        {
            _Click.Deflate(document);
        }
    }
}
