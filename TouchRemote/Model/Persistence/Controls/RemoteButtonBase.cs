using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;
using TouchRemote.Utils;
using TouchRemote.Web.Models;

namespace TouchRemote.Model.Persistence.Controls
{
    public abstract class RemoteButtonBase : RemoteElement
    {
        #region Dependency properties

        private IconHolder _Icon;
        [XmlIgnore]
        public virtual IconHolder Icon
        {
            get
            {
                return _Icon;
            }
            set
            {
                ChangeAndNotifyDependency(ref _Icon, value, () => Icon, NotifyIconChanged);
            }
        }

        private void NotifyIconChanged(object sender, PropertyChangedEventArgs args)
        {
            Notify(() => Icon);
        }

        private string _Label;
        [XmlIgnore]
        public virtual string Label
        {
            get
            {
                return _Label;
            }
            set
            {
                ChangeAndNotify(ref _Label, value, () => Label, (oldValue, newValue) => RenderedLabel = RenderText(newValue));
            }
        }

        private byte[] _RenderedLabel;
        [Browsable(false)]
        [XmlIgnore]
        public byte[] RenderedLabel
        {
            get
            {
                return _RenderedLabel;
            }
            set
            {
                ChangeAndNotify(ref _RenderedLabel, value, () => RenderedLabel);
            }
        }

        private Color _ActiveColor;
        [Category("Appearance")]
        [DisplayName("Active Color")]
        [Description("Foreground/text color for active/clicked/hovered states")]
        [XmlIgnore]
        public Color ActiveColor
        {
            get
            {
                return _ActiveColor;
            }
            set
            {
                ChangeAndNotify(ref _ActiveColor, value, () => ActiveColor);
            }
        }

        [Browsable(false)]
        [XmlAttribute("ActiveColor")]
        public string ActiveColorStr
        {
            get
            {
                return ActiveColor.ToHexString();
            }
            set
            {
                ActiveColor = (Color)ColorConverter.ConvertFromString(value);
            }
        }

        private Color _ActiveBackgroundColor;
        [Category("Appearance")]
        [DisplayName("Active Background Color")]
        [Description("Background color for active/clicked/hovered states")]
        [XmlIgnore]
        public Color ActiveBackgroundColor
        {
            get
            {
                return _ActiveBackgroundColor;
            }
            set
            {
                ChangeAndNotify(ref _ActiveBackgroundColor, value, () => ActiveBackgroundColor);
            }
        }

        [Browsable(false)]
        [XmlAttribute("ActiveBackgroundColor")]
        public string ActiveBackgroundColorStr
        {
            get
            {
                return ActiveBackgroundColor.ToHexString();
            }
            set
            {
                ActiveBackgroundColor = (Color)ColorConverter.ConvertFromString(value);
            }
        }

        private bool _WrapContents;
        [Category("Appearance")]
        [DisplayName("Wrap Contents")]
        [Description("Automatically wrap contents of controls to fit within the specified size. Only has an effect if AutoSize is unchecked.")]
        [XmlAttribute]
        public bool WrapContents
        {
            get
            {
                return _WrapContents;
            }
            set
            {
                ChangeAndNotify(ref _WrapContents, value, () => WrapContents);
            }
        }

        #endregion

        public RemoteButtonBase()
        {
            Icon = new IconHolder();
            ActiveBackgroundColorStr = "#FFEFEFEF";
            ActiveColor = Colors.Black;
        }

        #region Abstract interface

        protected abstract void HandleClick();

        #endregion

        #region Overrides

        [XmlIgnore]
        public override Dictionary<string, string> ControlStyle
        {
            get
            {
                var style = new Dictionary<string, string>
                {
                    { "overflow", "hidden" },
                    { "textOverflow", "ellipsis" }
                };
                if (WrapContents)
                {
                    style.Add("wordWrap", "break-word");
                    style.Add("overflowWrap", "break-word");
                    style.Add("whiteSpace", "pre-wrap");
                }
                else
                {
                    style.Add("whiteSpace", "pre");
                }
                return style;
            }
        }

        [XmlIgnore]
        public override Dictionary<string, string> WebProperties => new Dictionary<string, string>()
        {
            { "Label", Label },
            { "RenderedLabel", "data:image/png;base64," + Convert.ToBase64String(RenderedLabel) },
            { "ActiveColor", ActiveColor.ToCssString() },
            { "ActiveBackgroundColor", ActiveBackgroundColor.ToCssString() },
            { "IconData", Icon.Source != null ? "data:image/png;base64," + Convert.ToBase64String(Icon.Source.PngBytes) : "" },
        };

        [XmlIgnore]
        public override WebControl.WebControlType WebControlType => WebControl.WebControlType.Button;

        public override bool CanHandleEvent(string eventName)
        {
            return eventName == "click";
        }

        public override void ProcessEvent(string eventName, object eventData)
        {
            if (eventName == "click")
            {
                HandleClick();
            }
        }

        #endregion
    }
}
