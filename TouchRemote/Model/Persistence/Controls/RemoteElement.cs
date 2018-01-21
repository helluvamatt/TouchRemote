using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using TouchRemote.Lib;
using TouchRemote.Web.Models;
using TouchRemote.Utils;
using System.Xml.Serialization;
using System.Xml;

namespace TouchRemote.Model.Persistence.Controls
{
    [Serializable]
    [XmlInclude(typeof(RemoteBoundToggleButton))]
    [XmlInclude(typeof(RemoteToggleButton))]
    [XmlInclude(typeof(RemoteButton))]
    [XmlInclude(typeof(RemoteSlider))]
    [XmlType(Namespace = "http://schneenet.com/Controls.xsd")]
    public abstract class RemoteElement : INotifyPropertyChanged
    {
        #region Dependency properties

        private double _X;
        [Browsable(false)]
        [XmlAttribute]
        public double X
        {
            get
            {
                return _X;
            }
            set
            {
                ChangeAndNotify(ref _X, value, () => X);
            }
        }

        private double _Y;
        [Browsable(false)]
        [XmlAttribute]
        public double Y
        {
            get
            {
                return _Y;
            }
            set
            {
                ChangeAndNotify(ref _Y, value, () => Y);
            }
        }

        private int _ZIndex;
        [Browsable(false)]
        [XmlAttribute]
        public int ZIndex
        {
            get
            {
                return _ZIndex;
            }
            set
            {
                ChangeAndNotify(ref _ZIndex, value, () => ZIndex);
            }
        }

        #endregion

        [Browsable(false)]
        public event PropertyChangedEventHandler PropertyChanged;

        [Category("Data")]
        [DisplayName("ID")]
        [Description("GUID of the control")]
        [ReadOnly(true)]
        [XmlAttribute]
        public Guid Id { get; set; }

        #region Abstract interface

        [Browsable(false)]
        [XmlIgnore]
        public abstract Dictionary<string, string> WebProperties { get; }

        [Browsable(false)]
        [XmlIgnore]
        public abstract WebControl.WebControlType WebControlType { get; }

        public abstract bool CanHandleEvent(string eventName);

        public abstract void ProcessEvent(string eventName, object eventData);

        internal abstract InflateResult Inflate(PluginManager pluginManager);

        internal abstract void Deflate(XmlDocument document);

        #endregion

        #region Virtual interface

        public virtual void OnFloatBoundPropertyValueChanged(string propertyName, float newValue) { }
        public virtual void OnBooleanBoundPropertyValueChanged(string propertyName, bool newValue) { }
        public virtual void OnStringBoundPropertyValueChanged(string propertyName, string newValue) { }

        public virtual FloatBoundProperty GetFloatBoundProperty(string propertyName) { return null; }
        public virtual BooleanBoundProperty GetBooleanBoundProperty(string propertyName) { return null; }
        public virtual StringBoundProperty GetStringBoundProperty(string propertyName) { return null; }

        [Browsable(false)]
        [XmlIgnore]
        public virtual IEnumerable<string> BoundPropertyNames => new string[0];

        #endregion

        protected bool ChangeAndNotify<T>(ref T field, T value, Expression<Func<T>> memberExpression, Action<T, T> changedCallback = null)
        {
            return PropertyChanged.ChangeAndNotify(ref field, value, memberExpression, changedCallback);
        }

        protected bool ChangeAndNotifyDependency<T>(ref T field, T value, Expression<Func<T>> memberExpression, PropertyChangedEventHandler parentHandler, Action<T, T> changedCallback = null) where T : INotifyPropertyChanged
        {
            return PropertyChanged.ChangeAndNotifyDependency(ref field, value, memberExpression, parentHandler, changedCallback);
        }

        protected void Notify<T>(Expression<Func<T>> memberExpression)
        {
            PropertyChanged.Notify(memberExpression);
        }

        public override string ToString()
        {
            return string.Format("{0} [{1}]", GetType().Name, Id);
        }
    }
}
