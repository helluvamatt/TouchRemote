using System;
using System.Xml;
using System.Xml.Serialization;

namespace TouchRemote.Model.Persistence.Controls
{
    [Serializable]
    [XmlInclude(typeof(StringBoundPropertyInstance))]
    [XmlInclude(typeof(BooleanBoundPropertyInstance))]
    [XmlInclude(typeof(FloatBoundPropertyInstance))]
    [XmlInclude(typeof(ActionExecutableInstance))]
    [XmlType(Namespace = "http://schneenet.com/Controls.xsd")]
    public abstract class ImplInstance
    {
        private XmlElement anyField;

        private string typeField;

        [XmlAnyElement]
        public XmlElement Any
        {
            get
            {
                return anyField;
            }
            set
            {
                anyField = value;
            }
        }

        [XmlAttribute]
        public string Type
        {
            get
            {
                return typeField;
            }
            set
            {
                typeField = value;
            }
        }

        internal abstract InflateResult Inflate(PluginManager pluginManager);

        internal abstract void Deflate(XmlDocument document);
    }
}
