using FontAwesome.WPF;
using MahApps.Metro.Controls;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TouchRemote.Model.Persistence;
using TouchRemote.Model.Persistence.Controls;
using TouchRemote.Properties;
using TouchRemote.Utils;

namespace TouchRemote.UI.IconPicker
{
    /// <summary>
    /// Interaction logic for IconPicker.xaml
    /// </summary>
    internal partial class PickerWindow : MetroWindow
    {
        public IconManager IconManager { get; private set; }

        public ICommand BrowseCommand { get; private set; }

        public ICommand SetIconNullCommand { get; private set; }

        #region Dependency properties

        #region IconHolder

        public IconHolder IconHolder
        {
            get
            {
                return (IconHolder)GetValue(IconHolderProperty);
            }
            private set
            {
                SetValue(IconHolderProperty, value);
            }
        }

        public static readonly DependencyProperty IconHolderProperty = DependencyProperty.Register("IconHolder", typeof(IconHolder), typeof(PickerWindow));

        #endregion

        #region SelectedBuiltinIcon

        public BuiltinIcon SelectedBuiltinIcon
        {
            get
            {
                return (BuiltinIcon)GetValue(SelectedBuiltinIconProperty);
            }
            set
            {
                SetValue(SelectedBuiltinIconProperty, value);
            }
        }

        public static readonly DependencyProperty SelectedBuiltinIconProperty = DependencyProperty.Register("SelectedBuiltinIcon", typeof(BuiltinIcon), typeof(PickerWindow), new UIPropertyMetadata(OnSelectedBuiltinIconChanged));

        private static void OnSelectedBuiltinIconChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            var picker = dependencyObject as PickerWindow;
            if (picker.SelectedBuiltinIcon != null)
            {
                var iconHolder = new IconHolder() { Source = BuiltinIconSource.Create(picker.SelectedBuiltinIcon.Icon, picker.BuiltinIconColor), Name = picker.SelectedBuiltinIcon.ToString() };
                picker.IconHolder.Apply(iconHolder);
                picker.SelectedCustomIcon = null;
            }
        }

        #endregion

        #region SelectedCustomIcon

        public IconHolder SelectedCustomIcon
        {
            get
            {
                return (IconHolder)GetValue(SelectedCustomIconProperty);
            }
            set
            {
                SetValue(SelectedCustomIconProperty, value);
            }
        }

        public static readonly DependencyProperty SelectedCustomIconProperty = DependencyProperty.Register("SelectedCustomIcon", typeof(IconHolder), typeof(PickerWindow), new UIPropertyMetadata(OnSelectedCustomIconChanged));

        private static void OnSelectedCustomIconChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            var picker = dependencyObject as PickerWindow;
            if (picker.SelectedCustomIcon != null)
            {
                picker.IconHolder.Apply(picker.SelectedCustomIcon);
                picker.SelectedBuiltinIcon = null;
            }
        }

        #endregion

        #region BuiltinIconColor

        public Color BuiltinIconColor
        {
            get
            {
                return (Color)GetValue(BuiltinIconColorProperty);
            }
            set
            {
                SetValue(BuiltinIconColorProperty, value);
            }
        }

        public static readonly DependencyProperty BuiltinIconColorProperty = DependencyProperty.Register("BuiltinIconColor", typeof(Color), typeof(PickerWindow), new UIPropertyMetadata(OnBuiltinIconColorChanged));

        private static void OnBuiltinIconColorChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            var picker = dependencyObject as PickerWindow;
            var builtinSource = picker.IconHolder.Source as BuiltinIconSource;
            if (builtinSource != null)
            {
                builtinSource.SetColor(picker.BuiltinIconColor);
            }
        }

        #endregion

        #endregion

        public PickerWindow(IconHolder iconHolder, IconManager iconManager)
        {
            IconHolder = iconHolder ?? throw new ArgumentNullException("iconHolder");
            IconManager = iconManager ?? throw new ArgumentNullException("iconManager");
            BrowseCommand = new DelegateCommand(DoBrowse);
            SetIconNullCommand = new DelegateCommand(SetIconNull);

            // Setup dependency property initial values
            BuiltinIconColor = Colors.Black;
            if (IconHolder.Source != null)
            {
                var builtinSource = IconHolder.Source as BuiltinIconSource;
                if (builtinSource != null)
                {
                    BuiltinIconColor = (Color)ColorConverter.ConvertFromString(builtinSource.Color);
                    var icon = FontAwesomeIcon.None;
                    var iconStr = builtinSource.Icon;
                    if (!string.IsNullOrEmpty(iconStr) && Enum.TryParse(iconStr, out icon) && icon != FontAwesomeIcon.None)
                    {
                        SelectedBuiltinIcon = new BuiltinIcon(icon);
                    }
                    else
                    {
                        SetIconNull();
                    }
                }
                else
                {
                    SelectedCustomIcon = IconHolder;
                }
            }
            InitializeComponent();
        }

        private void DoBrowse()
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog();
            string lastDirectory = Settings.Default.LastOpenDirectory;
            if (string.IsNullOrEmpty(lastDirectory)) lastDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            openFileDialog.InitialDirectory = lastDirectory;
            openFileDialog.CheckFileExists = true;
            openFileDialog.Multiselect = false;
            openFileDialog.Title = "Choose image file...";
            openFileDialog.Filter = "";
            var result = openFileDialog.ShowDialog(this);
            if (result.HasValue && result.Value)
            {
                Settings.Default.LastOpenDirectory = Path.GetDirectoryName(openFileDialog.FileName);
                if (!IconManager.AddCustomImage(openFileDialog.FileName, true))
                {
                    MessageBox.Show(this, "There is already a custom icon with that file name.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SetIconNull()
        {
            SelectedBuiltinIcon = null;
            SelectedCustomIcon = null;
            var nullIcon = new IconHolder() { Name = "(None)" };
            IconHolder.Apply(nullIcon);
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBox = sender as ListBox;
            if (listBox != null) listBox.ScrollIntoView(listBox.SelectedItem);
        }
    }
}
