using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using TouchRemote.Lib;
using TouchRemote.Web.Models;
using TouchRemote.Utils;
using System.Xml.Serialization;
using System.Xml;
using System.Windows;
using System.Windows.Media;

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

        private bool _AutoSize;
        [XmlIgnore]
        [Category("Appearance")]
        [DisplayName("AutoSize")]
        [Description("Whether the control will grow and shrink based on its contents")]
        public bool AutoSize
        {
            get
            {
                return _AutoSize;
            }
            set
            {
                Width = value ? Double.NaN : ActualWidth;
                Height = value ? Double.NaN : ActualHeight;
                ChangeAndNotify(ref _AutoSize, value, () => AutoSize);
            }
        }

        private double _Width;
        //[Category("Appearance")]
        //[DisplayName("Width")]
        //[Description("Control width, ignored if AutoSize is true")]
        [Browsable(false)]
        [XmlIgnore]
        public double Width
        {
            get
            {
                return _Width;
            }
            set
            {
                ChangeAndNotify(ref _Width, value, () => Width);
            }
        }

        private double _Height;
        //[Category("Appearance")]
        //[DisplayName("Height")]
        //[Description("Control height, ignored if AutoSize is true")]
        [Browsable(false)]
        [XmlIgnore]
        public double Height
        {
            get
            {
                return _Height;
            }
            set
            {
                ChangeAndNotify(ref _Height, value, () => Height);
            }
        }

        private TextAlignment _TextAlignment;
        [Category("Appearance")]
        [DisplayName("Text Alignment")]
        [Description("Horizontal text alignment")]
        [XmlAttribute]
        public virtual TextAlignment TextAlignment
        {
            get
            {
                return _TextAlignment;
            }
            set
            {
                ChangeAndNotify(ref _TextAlignment, value, () => TextAlignment);
            }
        }

        private Color _Color;
        [Category("Appearance")]
        [DisplayName("Color")]
        [Description("Foreground/text color")]
        [XmlIgnore]
        public Color Color
        {
            get
            {
                return _Color;
            }
            set
            {
                ChangeAndNotify(ref _Color, value, () => Color);
            }
        }

        [Browsable(false)]
        [XmlAttribute("Color")]
        public string ColorStr
        {
            get
            {
                return Color.ToHexString();
            }
            set
            {
                Color = (Color)ColorConverter.ConvertFromString(value);
            }
        }

        private Color _BackgroundColor;
        [Category("Appearance")]
        [DisplayName("Background")]
        [Description("Control background color")]
        [XmlIgnore]
        public Color BackgroundColor
        {
            get
            {
                return _BackgroundColor;
            }
            set
            {
                ChangeAndNotify(ref _BackgroundColor, value, () => BackgroundColor);
            }
        }

        [Browsable(false)]
        [XmlAttribute("BackgroundColor")]
        public string BackgroundColorStr
        {
            get
            {
                return BackgroundColor.ToHexString();
            }
            set
            {
                BackgroundColor = (Color)ColorConverter.ConvertFromString(value);
            }
        }

        private Font _Font;
        [Category("Appearance")]
        [DisplayName("Font")]
        [Description("Label font")]
        [XmlElement("RemoteElement.Font")]
        public virtual Font Font
        {
            get
            {
                return _Font;
            }
            set
            {
                ChangeAndNotifyDependency(ref _Font, value, () => Font, OnFontChanged);
            }
        }

        private void OnFontChanged(object sender, PropertyChangedEventArgs args)
        {
            Notify(() => Font);
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

        [Browsable(false)]
        [XmlAttribute]
        public string Size
        {
            get
            {
                return AutoSize ? null : string.Format("{0},{1}", Width, Height);
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    string[] args = value.Split(',');
                    if (args.Length == 2)
                    {
                        double width, height;
                        if (double.TryParse(args[0], out width) && double.TryParse(args[1], out height))
                        {
                            ActualWidth = width;
                            ActualHeight = height;
                            AutoSize = false;
                            return;
                        }
                    }
                }
                AutoSize = true;
            }
        }

        [Browsable(false)]
        [XmlIgnore]
        public double ActualWidth { get; set; }

        [Browsable(false)]
        [XmlIgnore]
        public double ActualHeight { get; set; }

        public RemoteElement()
        {
            _AutoSize = true;
            _Width = _Height = Double.NaN;
            _Color = Colors.Black;
            _BackgroundColor = Colors.White;
            _TextAlignment = TextAlignment.Left;
            _Font = new Font();
        }

        #region Abstract interface

        [Browsable(false)]
        [XmlIgnore]
        public abstract Dictionary<string, string> ControlStyle { get; }

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

        [Browsable(false)]
        [XmlIgnore]
        public virtual int MaxControlTypeCount => 0;

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
