using FontAwesome.WPF;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;
using TouchRemote.Utils;

namespace TouchRemote.Model.Persistence.Controls
{
    [Serializable]
    [XmlInclude(typeof(BuiltinIconSource))]
    [XmlInclude(typeof(CustomIconSource))]
    [XmlType(Namespace = "http://schneenet.com/Controls.xsd")]
    public abstract class IconSource : INotifyPropertyChanged
    {
        [Browsable(false)]
        [XmlIgnore]
        public abstract BitmapSource Image { get; }

        [Browsable(false)]
        [XmlIgnore]
        public byte[] PngBytes { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RenderToPng()
        {
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(Image));
            using (MemoryStream stream = new MemoryStream())
            {
                encoder.Save(stream);
                PngBytes = stream.ToArray();
            }
            PropertyChanged.Notify(() => Image);
        }
    }

    [Serializable]
    [XmlType(Namespace = "http://schneenet.com/Controls.xsd")]
    public class BuiltinIconSource : IconSource
    {
        private string iconField;

        private string colorField;

        [XmlAttribute]
        public string Icon
        {
            get
            {
                return iconField;
            }
            set
            {
                iconField = value;
                RedrawImage();
            }
        }

        [XmlAttribute]
        public string Color
        {
            get
            {
                return colorField;
            }
            set
            {
                colorField = value;
                RedrawImage();
            }
        }

        public BuiltinIconSource() { }

        private BuiltinIconSource(string icon, string hexColor)
        {
            iconField = icon;
            colorField = hexColor;
            RedrawImage();
        }

        private BitmapSource _Image;
        [Browsable(false)]
        [XmlIgnore]
        public override BitmapSource Image => _Image;

        public static BuiltinIconSource Create(FontAwesomeIcon icon, Color color)
        {
            return new BuiltinIconSource(icon.ToString(), color.ToHexString());
        }

        public void SetColor(Color color)
        {
            Color = color.ToHexString();
        }

        private void RedrawImage()
        {
            FontAwesomeIcon icon = FontAwesomeIcon.None;
            Enum.TryParse(Icon, out icon);
            Color color = string.IsNullOrEmpty(Color) ? Colors.Black : (Color)ColorConverter.ConvertFromString(Color);
            var imageSource = ImageAwesome.CreateImageSource(icon, new SolidColorBrush(color));
            var drawingImage = new Image { Source = imageSource };
            drawingImage.Arrange(new Rect(0, 0, 48, 48));
            var img = new RenderTargetBitmap(48, 48, 96, 96, PixelFormats.Pbgra32);
            img.Render(drawingImage);
            _Image = img;
            RenderToPng();
        }

        public override bool Equals(object obj)
        {
            var other = obj as BuiltinIconSource;
            if (other == null) return false;
            return Icon == other.Icon && Color == other.Color;
        }

        public override int GetHashCode()
        {
            int hashCode = 17;
            hashCode = hashCode * 31 + Icon.GetHashCode();
            hashCode = hashCode * 31 + Color.GetHashCode();
            return hashCode;
        }
    }

    [Serializable]
    [XmlType(Namespace = "http://schneenet.com/Controls.xsd")]
    public class CustomIconSource : IconSource
    {
        private byte[] dataField;

        [XmlAttribute(DataType = "base64Binary")]
        public byte[] Data
        {
            get
            {
                return dataField;
            }
            set
            {
                dataField = value;
                RenderToPng();
            }
        }

        public override BitmapSource Image
        {
            get
            {
                using (MemoryStream byteStream = new MemoryStream(Data))
                {
                    BitmapImage source = new BitmapImage();
                    source.BeginInit();
                    source.CacheOption = BitmapCacheOption.OnLoad;
                    source.StreamSource = byteStream;
                    source.EndInit();
                    return source;
                }
            }
        }

        public override bool Equals(object obj)
        {
            var other = obj as CustomIconSource;
            return ByteArrayUtils.Equals(Data, other?.Data);
        }

        public override int GetHashCode()
        {
            return ByteArrayUtils.GetHashCode(Data);
        }
    }

    [Serializable]
    [XmlType(Namespace = "http://schneenet.com/Controls.xsd")]
    public class IconHolder : INotifyPropertyChanged
    {
        private IconSource sourceField;

        private string nameField;

        public IconSource Source
        {
            get
            {
                return sourceField;
            }
            set
            {
                PropertyChanged.ChangeAndNotifyDependency(ref sourceField, value, () => Source, OnSourcePropertyChanged);
            }
        }

        private void OnSourcePropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            PropertyChanged.Notify(() => Source);
        }

        [XmlAttribute]
        public string Name
        {
            get
            {
                return nameField;
            }
            set
            {
                PropertyChanged.ChangeAndNotify(ref nameField, value, () => Name);
            }
        }

        public void Apply(IconHolder other)
        {
            Source = other.Source;
            Name = other.Name;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public override bool Equals(object obj)
        {
            var other = obj as IconHolder;
            if (Source == null || other == null || other.Source == null) return false;
            return Source.Equals(other.Source);
        }

        public override int GetHashCode()
        {
            if (Source != null)
            {
                return Source.GetHashCode();
            }
            return 0;
        }
    }

}
