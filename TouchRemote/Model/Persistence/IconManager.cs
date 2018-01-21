using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FontAwesome.WPF;
using log4net;
using TouchRemote.Model.Persistence.Controls;

namespace TouchRemote.Model.Persistence
{
    internal class IconManager
    {
        #region Private members

        private const int ICON_SIZE = 64;

        private string _CustomIconsPath;

        private ILog _Log;

        #endregion

        public IEnumerable<BuiltinIcon> BuiltinIcons
        {
            get
            {
                return GetBuiltins().Select(icon => new BuiltinIcon(icon));
            }
        }

        public ObservableCollection<IconHolder> CustomIcons { get; private set; }

        public IconManager(string customIconsPath)
        {
            _Log = LogManager.GetLogger(GetType());
            _CustomIconsPath = customIconsPath;
            _Log.InfoFormat("Initializing IconManager... [CustomIconsPath = \"{0}\"]", _CustomIconsPath);
            Directory.CreateDirectory(_CustomIconsPath);
            CustomIcons = new ObservableCollection<IconHolder>();
            foreach (var file in Directory.GetFiles(_CustomIconsPath, "*.png"))
            {
                string path = Path.Combine(_CustomIconsPath, file);
                AddCustomImage(path, false);
            }
            _Log.InfoFormat("Loaded {0} custom icons.", CustomIcons.Count);
        }

        public bool AddCustomImage(string filePath, bool copy)
        {
            if (copy)
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                string newFilePath = Path.Combine(_CustomIconsPath, fileName + ".png");
                if (File.Exists(newFilePath)) return false;
                BitmapImage image = new BitmapImage();
                using (var stream = File.OpenRead(filePath))
                {
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.StreamSource = stream;
                    image.EndInit();
                }
                var encoder = new PngBitmapEncoder();
                if (image.PixelHeight > ICON_SIZE || image.PixelWidth > ICON_SIZE)
                {
                    // Uniform scale down, saved image will have a max dimension of ICON_SIZE
                    double scale = Math.Min((double)ICON_SIZE / image.PixelHeight, (double)ICON_SIZE / image.PixelWidth);
                    var resizedImage = new TransformedBitmap(image, new ScaleTransform(scale, scale));
                    encoder.Frames.Add(BitmapFrame.Create(resizedImage));
                }
                else
                {
                    encoder.Frames.Add(BitmapFrame.Create(image));
                }
                using (var stream = File.OpenWrite(newFilePath))
                {
                    encoder.Save(stream);
                }
                filePath = newFilePath;
            }
            var holder = new IconHolder();
            holder.Name = Path.GetFileNameWithoutExtension(filePath);
            holder.Source = new CustomIconSource() { Data = File.ReadAllBytes(filePath) };
            CustomIcons.Add(holder);
            return true;
        }

        private IEnumerable<FontAwesomeIcon> GetBuiltins()
        {
            return Enum.GetValues(typeof(FontAwesomeIcon))
                .Cast<FontAwesomeIcon>()
                .Where(icon => !IsAlias(icon) && icon != FontAwesomeIcon.None)
                .OrderBy(icon => icon.ToString());
        }

        private bool IsAlias(FontAwesomeIcon icon)
        {
            var type = typeof(FontAwesomeIcon);
            var memInfo = type.GetMember(icon.ToString());
            var attr = memInfo[0].GetCustomAttribute<IconAliasAttribute>();
            return attr != null;
        }
    }

    // Need a class representation of an enum so we can set null in the UI
    internal class BuiltinIcon
    {
        public BuiltinIcon(FontAwesomeIcon icon)
        {
            Icon = icon;
        }

        public FontAwesomeIcon Icon { get; private set; }

        public override string ToString()
        {
            return "Built-in: " + Icon.ToString();
        }

        public override bool Equals(object obj)
        {
            var other = obj as BuiltinIcon;
            return other != null && other.Icon == Icon;
        }

        public override int GetHashCode()
        {
            return (int)Icon;
        }
    }
}
