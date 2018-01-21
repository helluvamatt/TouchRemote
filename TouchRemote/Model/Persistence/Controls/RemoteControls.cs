using System;
using System.Xml.Serialization;

namespace TouchRemote.Model.Persistence.Controls
{
    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://schneenet.com/Controls.xsd")]
    [XmlRoot(Namespace = "http://schneenet.com/Controls.xsd", IsNullable = false)]
    public partial class RemoteControls
    {
        private RemoteElement[] itemsField;

        [XmlElement("BoundToggleButton", typeof(RemoteBoundToggleButton))]
        [XmlElement("Button", typeof(RemoteButton))]
        [XmlElement("Slider", typeof(RemoteSlider))]
        [XmlElement("ToggleButton", typeof(RemoteToggleButton))]
        public RemoteElement[] Items
        {
            get
            {
                return itemsField;
            }
            set
            {
                itemsField = value;
            }
        }
    }
}
