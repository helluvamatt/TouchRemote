using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using TouchRemote.Lib;
using System.Xml;
using System.Runtime.InteropServices;

namespace TouchRemote.BoundProperties
{
    [DisplayName("Active Window")]
    [Description("Returns the name of the currently active window")]
    public class ActiveWindowBoundProperty : StringBoundProperty
    {
        public bool SupportsValueChanged => false;

        // Not used because we don't support it (SupportsValueChanged => false)
        #pragma warning disable 0067
        public event BoundPropertyValueChangedHandler<string> ValueChanged;
        #pragma warning restore 0067

        public bool Initialize()
        {
            return true;
        }

        public string GetValue()
        {
            var hWnd = NativeInterop.GetForegroundWindow();
            if (hWnd == IntPtr.Zero) return null;
            // .NET framework will marshal an out LPTSTR into the string builder automatically
            var titleBuilder = new StringBuilder(10000);
            NativeInterop.GetWindowText(hWnd, titleBuilder, titleBuilder.Capacity);
            return titleBuilder.Length > 0 ? titleBuilder.ToString() : "None";
        }

        public void SetValue(string parameter)
        {
            // Not supported, but do not throw an exception
        }

        public void Dispose()
        {
            // Not needed
        }

        public void Deserialize(XmlElement element)
        {
            // Not needed
        }

        public void Serialize(XmlElement element)
        {
            // Not needed
        }

        private static class NativeInterop
        {
            [DllImport("user32.dll", SetLastError = true)]
            public static extern IntPtr GetForegroundWindow();

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
            public static extern int GetWindowText(IntPtr hWnd, StringBuilder wString, int nMaxCount);
        }
    }
}
