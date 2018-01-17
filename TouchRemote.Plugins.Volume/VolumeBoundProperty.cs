using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TouchRemote.Lib;

namespace TouchRemote.Plugins.Volume
{
    public class VolumeBoundProperty : ProvidedBoundProperty<int>
    {
        private const string NAME = "Volume - {0}";
        private const string DESCRIPTION = "Adjust the volume for \"{0}\"";

        private string _InterfaceName;

        public VolumeBoundProperty() { }

        internal VolumeBoundProperty(string interfaceName)
        {
            _InterfaceName = interfaceName;
        }

        public string Name => string.Format(NAME, _InterfaceName);

        public string Description => string.Format(DESCRIPTION, _InterfaceName);

        public void SetValue(int parameter)
        {
            // TODO
        }

        public int GetValue()
        {
            // TODO
            return 0;
        }

        public void Serialize(XmlElement element)
        {
            element.SetAttribute("Interface", _InterfaceName);
        }

        public void Deserialize(XmlElement element)
        {
            _InterfaceName = element.GetAttribute("Interface");
        }
    }
}
