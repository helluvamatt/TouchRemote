using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TouchRemote.Model
{
    internal class InterfaceModel
    {
        public string Name { get; private set; }
        public bool UnknownInterface { get; private set; }
        public IPAddress Address { get; private set; }
        public event Action<InterfaceModel> CheckedChanged;

        private bool _Checked;
        public bool Checked
        {
            get
            {
                return _Checked;
            }
            set
            {
                _Checked = value;
                CheckedChanged?.Invoke(this);
            }
        }

        public InterfaceModel(string name, IPAddress addr, bool check, bool unknownInterface, Action<InterfaceModel> changeCallback)
        {
            Name = name;
            Address = addr;
            _Checked = check;
            UnknownInterface = unknownInterface;
            CheckedChanged += changeCallback;
        }

        public string Label
        {
            get
            {
                return string.Format("{0} ({1})", Name, Address);
            }
        }
    }
}
