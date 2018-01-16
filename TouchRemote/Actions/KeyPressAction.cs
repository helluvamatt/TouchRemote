using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml;
using TouchRemote.Lib;

namespace TouchRemote.Actions
{
    [ActionName("Key Press")]
    [ActionDescription("Sends a global key event")]
    public class KeyPressAction : ActionExecutable
    {
        [ConfigProperty(Required = true)]
        [DisplayName("Alt modifier")]
        [Description("Key combination includes the Alt key")]
        [Category("Modifers")]
        public bool Alt { get; set; }

        [ConfigProperty(Required = true)]
        [DisplayName("Control modifier")]
        [Description("Key combination includes the Control key")]
        [Category("Modifers")]
        public bool Ctrl { get; set; }

        [ConfigProperty(Required = true)]
        [DisplayName("Shift modifier")]
        [Description("Key combination includes the Shift key")]
        [Category("Modifers")]
        public bool Shift { get; set; }

        [ConfigProperty(Required = true)]
        [DisplayName("Meta Key modifier")]
        [Description("Key combination includes the Meta/Super/Windows key")]
        [Category("Modifers")]
        public bool Meta { get; set; }

        [ConfigProperty(Required = true)]
        [DisplayName("Key")]
        [Description("Main key in the combination")]
        [Category("Key")]
        public Key Key { get; set; }

        public void Execute()
        {
            ushort vkKey = GetVirtualKey(Key);
            NativeInterop.SendKeyCombo(vkKey, Alt, Ctrl, Shift, Meta);
        }

        public void Deserialize(XmlElement element)
        {
            Alt = bool.Parse(element.GetAttribute("Alt"));
            Ctrl = bool.Parse(element.GetAttribute("Ctrl"));
            Shift = bool.Parse(element.GetAttribute("Shift"));
            Meta = bool.Parse(element.GetAttribute("Meta"));
            Key = (Key)Enum.Parse(typeof(Key), element.GetAttribute("Key"));
        }

        public void Serialize(XmlElement element)
        {
            element.SetAttribute("Alt", Alt.ToString());
            element.SetAttribute("Ctrl", Ctrl.ToString());
            element.SetAttribute("Shift", Shift.ToString());
            element.SetAttribute("Meta", Meta.ToString());
            element.SetAttribute("Key", Key.ToString());
        }

        private ushort GetVirtualKey(Key key)
        {
            return (ushort)KeyInterop.VirtualKeyFromKey(key);
        }

        private static class NativeInterop
        {
            [DllImport("TouchRemoteNative.dll")]
            public static extern void SendKey(ushort dwVirtualKey, bool bUp);

            [DllImport("TouchRemoteNative.dll")]
            public static extern void SendKeyCombo(ushort dwVirtualKey, bool bAlt, bool bCtrl, bool bShift, bool bMeta);
        }
    }
}
