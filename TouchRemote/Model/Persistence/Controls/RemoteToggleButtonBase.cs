using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace TouchRemote.Model.Persistence.Controls
{
    public abstract class RemoteToggleButtonBase : RemoteButtonBase
    {
        #region Dependency properties

        private IconHolder _IconOn;
        [Category("Appearance")]
        [DisplayName("On Icon")]
        [Description("Button icon when the button is toggled on")]
        [XmlElement("RemoteToggleButtonBase.IconOn")]
        public IconHolder IconOn
        {
            get
            {
                return _IconOn;
            }
            set
            {
                ChangeAndNotifyDependency(ref _IconOn, value, () => IconOn, OnIconOnChanged, (oldValue, newValue) => IconsUpdated());
            }
        }

        private void OnIconOnChanged(object sender, PropertyChangedEventArgs args)
        {
            Notify(() => IconOn);
            IconsUpdated();
        }

        private IconHolder _IconOff;
        [Category("Appearance")]
        [DisplayName("Off Icon")]
        [Description("Button icon when the button is toggled off")]
        [XmlElement("RemoteToggleButtonBase.IconOff")]
        public IconHolder IconOff
        {
            get
            {
                return _IconOff;
            }
            set
            {
                ChangeAndNotifyDependency(ref _IconOff, value, () => IconOff, OnIconOffChanged, (oldValue, newValue) => IconsUpdated());
            }
        }

        private void OnIconOffChanged(object sender, PropertyChangedEventArgs args)
        {
            Notify(() => IconOff);
            IconsUpdated();
        }

        private string _LabelOn;
        [Category("Appearance")]
        [DisplayName("On Label")]
        [Description("Button label text when the button is toggled on")]
        [XmlAttribute("LabelOn")]
        public string LabelOn
        {
            get
            {
                return _LabelOn;
            }
            set
            {
                ChangeAndNotify(ref _LabelOn, value, () => LabelOn, (oldValue, newValue) => LabelsUpdated());
            }
        }

        private string _LabelOff;
        [Category("Appearance")]
        [DisplayName("Off Label")]
        [Description("Button label text when the button is toggled off")]
        [XmlAttribute("LabelOff")]
        public string LabelOff
        {
            get
            {
                return _LabelOff;
            }
            set
            {
                ChangeAndNotify(ref _LabelOff, value, () => LabelOff, (oldValue, newValue) => LabelsUpdated());
            }
        }

        [Browsable(false)]
        [XmlIgnore]
        public override IconHolder Icon { get => base.Icon; set => base.Icon = value; }

        [Browsable(false)]
        [XmlIgnore]
        public override string Label { get => base.Label; set => base.Label = value; }

        private bool _Toggled;
        [Browsable(false)]
        [XmlIgnore]
        public bool Toggled
        {
            get
            {
                return _Toggled;
            }
            protected set
            {
                ChangeAndNotify(ref _Toggled, value, () => Toggled, (oldValue, newValue) => { IconsUpdated(); LabelsUpdated(); });
            }
        }

        private void LabelsUpdated()
        {
            Label = Toggled ? LabelOn : LabelOff;
        }

        private void IconsUpdated()
        {
            Icon = Toggled ? IconOn : IconOff;
        }

        #endregion

        public RemoteToggleButtonBase()
        {
            IconOn = new IconHolder();
            IconOff = new IconHolder();
        }

        protected abstract void HandleToggle();

        protected sealed override void HandleClick()
        {
            Toggled = !Toggled;
            HandleToggle();
        }
    }
}
