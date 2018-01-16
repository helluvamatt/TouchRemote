using FontAwesome.WPF;
using System.Windows;
using System.ComponentModel;
using TouchRemote.Lib;
using System;
using System.Xml.Serialization;
using System.Xml;
using System.Xml.Schema;
using TouchRemote.UI.ConfigEditor.Model;
using System.Collections.Generic;

namespace TouchRemote.Model
{
    public class Button : Element
    {
        #region Dependency properties

        public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(FontAwesomeIcon), typeof(Button));
        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register("Label", typeof(string), typeof(Button));
        public static readonly DependencyProperty ClickActionProperty = DependencyProperty.Register("ClickAction", typeof(IActionExecutableDescriptor), typeof(Button), new UIPropertyMetadata(ClickActionChanged));
        public static readonly DependencyProperty ClickActionImplProperty = DependencyProperty.Register("ClickActionImpl", typeof(ActionExecutable), typeof(Button));

        private static void ClickActionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var btn = obj as Button;
            if (btn.ClickAction != null)
            {
                btn.ClickActionImpl = btn.ClickAction.Manager.GetActionInstance(btn.ClickAction);
            }
            else
            {
                btn.ClickActionImpl = null;
            }
        }

        #endregion

        [Category("Appearance")]
        [DisplayName("Icon")]
        [Description("Button icon")]
        public FontAwesomeIcon Icon
        {
            get
            {
                return (FontAwesomeIcon)GetValue(IconProperty);
            }
            set
            {
                SetValue(IconProperty, value);
            }
        }

        [Category("Appearance")]
        [DisplayName("Label")]
        [Description("Button label text")]
        public string Label
        {
            get
            {
                return (string)GetValue(LabelProperty);
            }
            set
            {
                SetValue(LabelProperty, value);
            }
        }

        [Category("Behavior")]
        [DisplayName("Click Action")]
        [Description("Action to perform when the button is clicked.")]
        public IActionExecutableDescriptor ClickAction
        {
            get
            {
                return (IActionExecutableDescriptor)GetValue(ClickActionProperty);
            }
            set
            {
                SetValue(ClickActionProperty, value);
            }
        }

        [Category("Behavior")]
        [DisplayName("Click Action Properties")]
        [Description("Configuration for the click action")]
        public ActionExecutable ClickActionImpl
        {
            get
            {
                return (ActionExecutable)GetValue(ClickActionImplProperty);
            }
            set
            {
                SetValue(ClickActionImplProperty, value);
            }
        }

        public override Dictionary<string, string> WebProperties
        {
            get
            {
                var conv = new FontAwesome.WPF.Converters.CssClassNameConverter() { Mode = FontAwesome.WPF.Converters.CssClassConverterMode.FromIconToString };
                var iconClass = (string)conv.Convert(Icon, typeof(string), null, null);
                return new Dictionary<string, string>()
                {
                    { "Label", Label },
                    { "IconClass", iconClass },
                };
            }
        }

        public virtual void Click()
        {
            if (ClickActionImpl != null)
            {
                ClickActionImpl.Execute();
            }
        }

        public void NotifyClickActionImplChanged()
        {
            OnPropertyChanged(new DependencyPropertyChangedEventArgs(ClickActionImplProperty, null, ClickActionImpl));
        }
    }
}
