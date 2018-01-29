using System;
using System.Xml;
using System.Xml.Serialization;
using System.ComponentModel;
using TouchRemote.Utils;
using System.Text;
using System.Windows.Media;
using System.Windows;

namespace TouchRemote.Model.Persistence.Controls
{
    [Serializable]
    [XmlType(Namespace = "http://schneenet.com/Controls.xsd")]
    public class Font : INotifyPropertyChanged
    {
        private static readonly FontStyleConverter fontStyleConverter = new FontStyleConverter();
        private static readonly FontWeightConverter fontWeightConverter = new FontWeightConverter();
        private static readonly FontStretchConverter fontStretchConverter = new FontStretchConverter();

        public Font()
        {
            _Family = new FontFamily("Segoe UI");
            _Size = 10;
            _Weight = FontWeights.Normal;
            _Style = FontStyles.Normal;
            _Stretch = FontStretches.Normal;
        }

        private FontFamily _Family;
        [XmlIgnore]
        public FontFamily Family
        {
            get
            {
                return _Family;
            }
            set
            {
                PropertyChanged.ChangeAndNotify(ref _Family, value, () => Family);
            }
        }

        private double _Size;
        [XmlAttribute]
        public double Size
        {
            get
            {
                return _Size;
            }
            set
            {
                PropertyChanged.ChangeAndNotify(ref _Size, value, () => Size);
            }
        }

        private FontWeight _Weight;
        [XmlIgnore]
        public FontWeight Weight
        {
            get
            {
                return _Weight;
            }
            set
            {
                PropertyChanged.ChangeAndNotify(ref _Weight, value, () => Weight);
            }
        }

        private FontStyle _Style;
        [XmlIgnore]
        public FontStyle Style
        {
            get
            {
                return _Style;
            }
            set
            {
                PropertyChanged.ChangeAndNotify(ref _Style, value, () => Style);
            }
        }

        private FontStretch _Stretch;
        [XmlIgnore]
        public FontStretch Stretch
        {
            get
            {
                return _Stretch;
            }
            set
            {
                PropertyChanged.ChangeAndNotify(ref _Stretch, value, () => Stretch);
            }
        }

        [XmlAttribute("Family")]
        public string FamilyStr
        {
            get
            {
                return Family.Source;
            }
            set
            {
                Family = new FontFamily(value);
            }
        }

        [XmlAttribute("Style")]
        public string StyleStr
        {
            get
            {
                return fontStyleConverter.ConvertToString(Style);
            }
            set
            {
                Style = (FontStyle)fontStyleConverter.ConvertFromString(value);
            }
        }

        [XmlAttribute("Weight")]
        public string WeightStr
        {
            get
            {
                return fontWeightConverter.ConvertToString(Weight);
            }
            set
            {
                Weight = (FontWeight)fontWeightConverter.ConvertFromString(value);
            }
        }

        [XmlAttribute("Stretch")]
        public string StretchStr
        {
            get
            {
                return fontStretchConverter.ConvertToString(Stretch);
            }
            set
            {
                Stretch = (FontStretch)fontStretchConverter.ConvertFromString(value);
            }
        }

        [XmlIgnore]
        public bool IsDefault
        {
            get
            {
                return Family.Source == "Segoe UI" && Size == 10 && Weight == FontWeights.Normal && Style == FontStyles.Normal && Stretch == FontStretches.Normal;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public override string ToString()
        {
            var str = new StringBuilder();
            str.AppendFormat("{0}, {1}", Family, Size);
            if (Weight != FontWeights.Normal) str.AppendFormat(", {0}", WeightStr);
            if (Style != FontStyles.Normal) str.AppendFormat(", {0}", StyleStr);
            if (Stretch != FontStretches.Normal) str.AppendFormat(", {0}", StretchStr);
            return str.ToString();
        }
    }
}
