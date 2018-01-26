using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;
using TouchRemote.Web.Models;
using TouchRemote.Utils;
using System.Windows.Media;

namespace TouchRemote.Model.Persistence.Controls
{
    public abstract class RemoteLabelBase : RemoteElement
    {
        #region Dependency properties

        private string _Text;
        [XmlIgnore]
        [Category("Data")]
        [DisplayName("Current Text")]
        [Description("The current text displayed")]
        [ReadOnly(true)]
        public virtual string Text
        {
            get
            {
                return _Text;
            }
            set
            {
                ChangeAndNotify(ref _Text, value, () => Text);
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
        public override Dictionary<string, string> WebProperties => new Dictionary<string, string>
        {
            { "Text", Text }
        };

        [XmlIgnore]
        public override WebControl.WebControlType WebControlType => WebControl.WebControlType.Label;

        public override bool CanHandleEvent(string eventName)
        {
            return false;
        }

        public override void ProcessEvent(string eventName, object eventData)
        {
            // No-op
        }

        #endregion
    }
}
