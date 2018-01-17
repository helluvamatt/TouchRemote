using FontAwesome.WPF;
using System;
using System.ComponentModel;
using System.Windows;
using TouchRemote.Lib;

namespace TouchRemote.Model
{
    public class ToggleButton : Button
    {
        #region Dependency properties

        public static readonly DependencyProperty ToggledProperty = DependencyProperty.Register("Toggled", typeof(bool), typeof(ToggleButton), new UIPropertyMetadata(ToggledChanged));
        public static readonly DependencyProperty IconOnProperty = DependencyProperty.Register("IconOn", typeof(FontAwesomeIcon), typeof(ToggleButton), new UIPropertyMetadata(IconChanged));
        public static readonly DependencyProperty IconOffProperty = DependencyProperty.Register("IconOff", typeof(FontAwesomeIcon), typeof(ToggleButton), new UIPropertyMetadata(IconChanged));
        public static readonly DependencyProperty LabelOnProperty = DependencyProperty.Register("LabelOn", typeof(string), typeof(ToggleButton), new UIPropertyMetadata(LabelChanged));
        public static readonly DependencyProperty LabelOffProperty = DependencyProperty.Register("LabelOff", typeof(string), typeof(ToggleButton), new UIPropertyMetadata(LabelChanged));
        public static readonly DependencyProperty ToggleOnActionProperty = DependencyProperty.Register("ToggleOnAction", typeof(IActionExecutableDescriptor), typeof(ToggleButton), new UIPropertyMetadata(ToggleOnActionChanged));
        public static readonly DependencyProperty ToggleOnActionImplProperty = DependencyProperty.Register("ToggleOnActionImpl", typeof(ActionExecutable), typeof(ToggleButton));
        public static readonly DependencyProperty ToggleOffActionProperty = DependencyProperty.Register("ToggleOffAction", typeof(IActionExecutableDescriptor), typeof(ToggleButton), new UIPropertyMetadata(ToggleOffActionChanged));
        public static readonly DependencyProperty ToggleOffActionImplProperty = DependencyProperty.Register("ToggleOffActionImpl", typeof(ActionExecutable), typeof(ToggleButton));

        private static void ToggledChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            IconChanged(sender, args);
            LabelChanged(sender, args);
        }

        private static void IconChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var tb = sender as ToggleButton;
            tb.Icon = tb.Toggled ? tb.IconOn : tb.IconOff;
        }

        private static void LabelChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var tb = sender as ToggleButton;
            tb.Label = tb.Toggled ? tb.LabelOn : tb.LabelOff;
        }

        private static void ToggleOnActionChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var tb = sender as ToggleButton;
            if (tb.ToggleOnAction != null)
            {
                tb.ToggleOnActionImpl = tb.ToggleOnAction.Manager.GetActionInstance(tb.ToggleOnAction);
            }
            else
            {
                tb.ToggleOnActionImpl = null;
            }
        }

        private static void ToggleOffActionChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var tb = sender as ToggleButton;
            if (tb.ToggleOffAction != null)
            {
                tb.ToggleOffActionImpl = tb.ToggleOffAction.Manager.GetActionInstance(tb.ToggleOffAction);
            }
            else
            {
                tb.ToggleOffActionImpl = null;
            }
        }

        #endregion

        [Category("Behavior")]
        [DisplayName("Toggle State")]
        [Description("Current toggled state: changed when the button is clicked from the remote.")]
        public bool Toggled
        {
            get
            {
                return (bool)GetValue(ToggledProperty);
            }
            private set
            {
                SetValue(ToggledProperty, value);
            }
        }

        [Category("Appearance")]
        [DisplayName("On Icon")]
        [Description("Button icon when the button is toggled on")]
        public FontAwesomeIcon IconOn
        {
            get
            {
                return (FontAwesomeIcon)GetValue(IconOnProperty);
            }
            set
            {
                SetValue(IconOnProperty, value);
            }
        }

        [Category("Appearance")]
        [DisplayName("Off Icon")]
        [Description("Button icon when the button is toggled off")]
        public FontAwesomeIcon IconOff
        {
            get
            {
                return (FontAwesomeIcon)GetValue(IconOffProperty);
            }
            set
            {
                SetValue(IconOffProperty, value);
            }
        }

        [Category("Appearance")]
        [DisplayName("On Label")]
        [Description("Button label text when the button is toggled on")]
        public string LabelOn
        {
            get
            {
                return (string)GetValue(LabelOnProperty);
            }
            set
            {
                SetValue(LabelOnProperty, value);
            }
        }

        [Category("Appearance")]
        [DisplayName("Off Label")]
        [Description("Button label text when the button is toggled off")]
        public string LabelOff
        {
            get
            {
                return (string)GetValue(LabelOffProperty);
            }
            set
            {
                SetValue(LabelOffProperty, value);
            }
        }

        [Category("Behavior")]
        [DisplayName("Toggle On Action")]
        [Description("Action to perform when the button is toggled on.")]
        public IActionExecutableDescriptor ToggleOnAction
        {
            get
            {
                return (IActionExecutableDescriptor)GetValue(ToggleOnActionProperty);
            }
            set
            {
                SetValue(ToggleOnActionProperty, value);
            }
        }

        [Category("Behavior")]
        [DisplayName("Toggle On Action Properties")]
        [Description("Configuration for the toggle on action")]
        public ActionExecutable ToggleOnActionImpl
        {
            get
            {
                return (ActionExecutable)GetValue(ToggleOnActionImplProperty);
            }
            set
            {
                SetValue(ToggleOnActionImplProperty, value);
            }
        }

        [Category("Behavior")]
        [DisplayName("Toggle Off Action")]
        [Description("Action to perform when the button is toggled off.")]
        public IActionExecutableDescriptor ToggleOffAction
        {
            get
            {
                return (IActionExecutableDescriptor)GetValue(ToggleOffActionProperty);
            }
            set
            {
                SetValue(ToggleOffActionProperty, value);
            }
        }

        [Category("Behavior")]
        [DisplayName("Toggle Off Action Properties")]
        [Description("Configuration for the toggle off action")]
        public ActionExecutable ToggleOffActionImpl
        {
            get
            {
                return (ActionExecutable)GetValue(ToggleOffActionImplProperty);
            }
            set
            {
                SetValue(ToggleOffActionImplProperty, value);
            }
        }

        protected override void Click()
        {
            Toggled = !Toggled;
            if (Toggled)
            {
                if (ToggleOnActionImpl != null)
                {
                    ToggleOnActionImpl.Execute();
                }
            }
            else
            {
                if (ToggleOffActionImpl != null)
                {
                    ToggleOffActionImpl.Execute();
                }
            }
        }

        public void NotifyToggleOnActionImplChanged()
        {
            OnPropertyChanged(new DependencyPropertyChangedEventArgs(ToggleOnActionImplProperty, null, ToggleOnActionImpl));
        }

        public void NotifyToggleOffActionImplChanged()
        {
            OnPropertyChanged(new DependencyPropertyChangedEventArgs(ToggleOffActionImplProperty, null, ToggleOffActionImpl));
        }

    }
}
