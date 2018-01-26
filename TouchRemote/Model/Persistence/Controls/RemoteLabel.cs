using System;
using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;

namespace TouchRemote.Model.Persistence.Controls
{
    [Serializable]
    [XmlType(Namespace = "http://schneenet.com/Controls.xsd")]
    public sealed class RemoteLabel : RemoteLabelBase
    {
        [XmlElement("RemoteLabel.Text")]
        [Category("Data")]
        [DisplayName("Text")]
        [Description("Text to be displayed by the label")]
        [ReadOnly(false)]
        public override string Text { get => base.Text; set => base.Text = value; }

        internal override void Deflate(XmlDocument document)
        {
            // No-op
        }

        internal override InflateResult Inflate(PluginManager pluginManager)
        {
            return InflateResult.Default;
        }
    }
}
