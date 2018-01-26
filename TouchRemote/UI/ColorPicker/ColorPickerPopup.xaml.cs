using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using WMColor = System.Windows.Media.Color;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TouchRemote.Utils;
using System.Windows.Controls.Primitives;
using System.ComponentModel;
using Spectrum;

namespace TouchRemote.UI.ColorPicker
{
    /// <summary>
    /// Interaction logic for ColorPickerPopup.xaml
    /// </summary>
    public partial class ColorPickerPopup : UserControl, INotifyPropertyChanged
    {
        #region Dependency properties

        public static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register("IsOpen", typeof(bool), typeof(ColorPickerPopup));
        public static readonly DependencyProperty SelectedColorProperty = DependencyProperty.Register("SelectedColor", typeof(WMColor), typeof(ColorPickerPopup), new UIPropertyMetadata(OnSelectedColorUpdated));

        private static void OnSelectedColorUpdated(DependencyObject @object, DependencyPropertyChangedEventArgs args)
        {
            var cp = @object as ColorPickerPopup;
            cp.RecalculateFromSelected();
        }

        public bool IsOpen
        {
            get
            {
                return (bool)GetValue(IsOpenProperty);
            }
            set
            {
                SetValue(IsOpenProperty, value);
            }
        }

        public WMColor SelectedColor
        {
            get
            {
                return (WMColor)GetValue(SelectedColorProperty);
            }
            set
            {
                SetValue(SelectedColorProperty, value);
            }
        }

        private double _Hue;
        public double Hue
        {
            get
            {
                return _Hue;
            }
            set
            {
                PropertyChanged.ChangeAndNotify(ref _Hue, value, () => Hue, (oldVal, newVal) => RecalculateFromHsv());
                RecalculateFromHsv();
            }
        }

        private double _Sat;
        public double Sat
        {
            get
            {
                return _Sat;
            }
            set
            {
                PropertyChanged.ChangeAndNotify(ref _Sat, value, () => Sat, (oldVal, newVal) => RecalculateFromHsv());
                RecalculateFromHsv();
            }
        }

        private double _Val;
        public double Val
        {
            get
            {
                return _Val;
            }
            set
            {
                PropertyChanged.ChangeAndNotify(ref _Val, value, () => Val, (oldVal, newVal) => RecalculateFromHsv());
            }
        }

        private byte _R;
        public byte R
        {
            get
            {
                return _R;
            }
            set
            {
                PropertyChanged.ChangeAndNotify(ref _R, value, () => R, (oldVal, newVal) => RecalculateFromRgb());
            }
        }

        private byte _G;
        public byte G
        {
            get
            {
                return _G;
            }
            set
            {
                PropertyChanged.ChangeAndNotify(ref _G, value, () => G, (oldVal, newVal) => RecalculateFromRgb());
            }
        }

        private byte _B;
        public byte B
        {
            get
            {
                return _B;
            }
            set
            {
                PropertyChanged.ChangeAndNotify(ref _B, value, () => B, (oldVal, newVal) => RecalculateFromRgb());
            }
        }

        private byte _A;
        public byte A
        {
            get
            {
                return _A;
            }
            set
            {
                PropertyChanged.ChangeAndNotify(ref _A, value, () => A, (oldVal, newVal) => RecalculateFromAlpha());
            }
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        public ColorPickerPopup()
        {
            InitializeComponent();
        }

        private void toggleButton_Click(object sender, RoutedEventArgs e)
        {
            IsOpen = !IsOpen;
        }

        #region Color property routing

        private bool recalculating = false;

        private void RecalculateFromSelected()
        {
            if (recalculating) return;
            recalculating = true;
            A = SelectedColor.A;
            R = SelectedColor.R;
            G = SelectedColor.G;
            B = SelectedColor.B;
            var hsv = SelectedColor.ToHSV();
            Hue = hsv.H;
            Sat = hsv.S;
            Val = hsv.V;
            recalculating = false;
        }

        private void RecalculateFromRgb()
        {
            if (recalculating) return;
            recalculating = true;
            SelectedColor = WMColor.FromArgb(A, R, G, B);
            var hsv = SelectedColor.ToHSV();
            Hue = hsv.H;
            Sat = hsv.S;
            Val = hsv.V;
            recalculating = false;
        }

        private void RecalculateFromHsv()
        {
            if (recalculating) return;
            recalculating = true;
            var hsv = new Color.HSV(Hue, Sat, Val);
            SelectedColor = hsv.ToWpfColor();
            R = SelectedColor.R;
            G = SelectedColor.G;
            B = SelectedColor.B;
            recalculating = false;
        }

        private void RecalculateFromAlpha()
        {
            if (recalculating) return;
            recalculating = true;
            SelectedColor = WMColor.FromArgb(A, R, G, B);
            recalculating = false;
        }

        #endregion
    }
}
