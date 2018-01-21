using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;
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
                ChangeAndNotify(ref _Label, value, () => Label);
            }
        }

        #endregion

        public RemoteButtonBase()
        {
            Icon = new IconHolder();
        }

        #region Abstract interface

        protected abstract void HandleClick();

        #endregion

        #region Overrides

        [XmlIgnore]
        public override Dictionary<string, string> WebProperties
        {
            get
            {
                return new Dictionary<string, string>()
                {
                    { "Label", Label },
                    { "IconData", "data:image/png;base64," + Convert.ToBase64String(Icon.Source.PngBytes) },
                };
            }
        }

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
