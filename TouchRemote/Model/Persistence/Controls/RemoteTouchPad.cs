using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using System.Xml.Serialization;
using TouchRemote.Web.Models;

namespace TouchRemote.Model.Persistence.Controls
{
    [XmlType(Namespace = "http://schneenet.com/Control.xsd")]
    public sealed class RemoteTouchPad : RemoteElement
    {
        private double _Sensitivity;
        [DisplayName("Sensitivity")]
        [Description("Mouse movement sensitivity, increase if the mouse is moving too slowly, descrease if the mouse is moving to fast.")]
        [Category("Behavior")]
        [XmlAttribute]
        public double Sensitivity
        {
            get
            {
                return _Sensitivity;
            }
            set
            {
                ChangeAndNotify(ref _Sensitivity, value, () => Sensitivity);
            }
        }

        
        private bool _ShowMiddleMouseButton;
        [DisplayName("Show Middle Mouse Button")]
        [Description("Show a click target for the middle mouse button.")]
        [Category("Appearance")]
        [XmlAttribute]
        public bool ShowMiddleMouseButton
        {
            get
            {
                return _ShowMiddleMouseButton;
            }
            set
            {
                ChangeAndNotify(ref _ShowMiddleMouseButton, value, () => ShowMiddleMouseButton);
            }
        }

        private int _ClickTargetHeight;
        [DisplayName("Button Height")]
        [Description("Height of buttons in the control.")]
        [Category("Appearance")]
        [XmlAttribute]
        public int ClickTargetHeight
        {
            get
            {
                return _ClickTargetHeight;
            }
            set
            {
                ChangeAndNotify(ref _ClickTargetHeight, value, () => ClickTargetHeight);
            }
        }

        private bool _TapToClick;
        [DisplayName("Tap to Click")]
        [Description("Send a left click when tapping on the movement area.")]
        [Category("Behavior")]
        [XmlAttribute]
        public bool TapToClick
        {
            get
            {
                return _TapToClick;
            }
            set
            {
                ChangeAndNotify(ref _TapToClick, value, () => TapToClick);
            }
        }

        private bool _AllowScrolling;
        [DisplayName("Allow Scrolling")]
        [Description("Two finger drag on the movement area will send mouse wheel scroll events.")]
        [Category("Behavior")]
        [XmlAttribute]
        public bool AllowScrolling
        {
            get
            {
                return _AllowScrolling;
            }
            set
            {
                ChangeAndNotify(ref _AllowScrolling, value, () => AllowScrolling);
            }
        }

        private bool _ReverseScrolling;
        [DisplayName("Reverse Scrolling")]
        [Description("Reverse the scroll direction. Normal scroll behavior acts like a touch screen, dragging the content in the direction of the finger drag. Check this to reverse this behavior.")]
        [Category("Behavior")]
        [XmlAttribute]
        public bool ReverseScrolling
        {
            get
            {
                return _ReverseScrolling;
            }
            set
            {
                ChangeAndNotify(ref _ReverseScrolling, value, () => ReverseScrolling);
            }
        }

        #region Hidden properties

        [Browsable(false)]
        [XmlIgnore]
        public override Font Font { get => base.Font; set => base.Font = value; }

        [Browsable(false)]
        [XmlIgnore]
        public override TextAlignment TextAlignment { get => base.TextAlignment; set => base.TextAlignment = value; }

        #endregion

        public RemoteTouchPad()
        {
            _Sensitivity = 1.0;
            _ClickTargetHeight = 48;
            _ShowMiddleMouseButton = false;
            BackgroundColorStr = "#FFDEDEDE";
            ColorStr = "#FF555555";
        }

        [XmlIgnore]
        [Browsable(false)]
        public override Dictionary<string, string> ControlStyle => new Dictionary<string, string> { };

        [XmlIgnore]
        [Browsable(false)]
        public override Dictionary<string, string> WebProperties => new Dictionary<string, string> {
            { "ShowMiddleMouseButton", ShowMiddleMouseButton.ToString() },
            { "ClickTargetHeight", ClickTargetHeight.ToString() },
            { "AllowScrolling", AllowScrolling.ToString() },
            { "TapToClick", TapToClick.ToString() }
        };

        [XmlIgnore]
        [Browsable(false)]
        public override WebControl.WebControlType WebControlType => WebControl.WebControlType.TouchPad;

        [XmlIgnore]
        [Browsable(false)]
        public override int MaxControlTypeCount => 1;

        public override bool CanHandleEvent(string eventName)
        {
            return eventName == "mouse-move"
                || eventName == "mouse-scroll"
                || eventName == "mouse-click-left"
                || eventName == "mouse-click-middle"
                || eventName == "mouse-click-right";
        }

        public override void ProcessEvent(string eventName, object eventData)
        {
            if (eventName == "mouse-move")
            {
                var d = eventData as JObject;
                var dX = d["dX"].Value<int>();
                var dY = d["dY"].Value<int>();
                if (Sensitivity != 1)
                {
                    dX = Convert.ToInt32(Math.Round(Sensitivity * dX));
                    dY = Convert.ToInt32(Math.Round(Sensitivity * dY));
                }
                Interop.SendMouseMove(dX, dY);
            }
            else if (eventName == "mouse-scroll")
            {
                var dY = (long)eventData * 5;
                if (ReverseScrolling) dY = -dY;
                if (dY <= int.MaxValue && dY >= int.MinValue)
                {
                    Interop.SendMouseScroll(Convert.ToInt32(dY));
                }
            }
            else if (eventName == "mouse-click-left")
            {
                Interop.SendMouseClickLeft();
            }
            else if (eventName == "mouse-click-middle")
            {
                Interop.SendMouseClickMiddle();
            }
            else if (eventName == "mouse-click-right")
            {
                Interop.SendMouseClickRight();
            }
        }

        internal override void Deflate(XmlDocument document)
        {
            // No-op
        }

        internal override InflateResult Inflate(PluginManager pluginManager)
        {
            return InflateResult.Default;
        }

        private static class Interop
        {
            [DllImport("TouchRemoteNative.dll")]
            public static extern void SendMouseMove(int dX, int dY);

            [DllImport("TouchRemoteNative.dll")]
            public static extern void SendMouseScroll(int wheelDelta);

            [DllImport("TouchRemoteNative.dll")]
            public static extern void SendMouseClickLeft();

            [DllImport("TouchRemoteNative.dll")]
            public static extern void SendMouseClickMiddle();

            [DllImport("TouchRemoteNative.dll")]
            public static extern void SendMouseClickRight();
        }
    }
}
