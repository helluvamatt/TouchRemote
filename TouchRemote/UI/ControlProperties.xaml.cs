using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using TouchRemote.Model;
using TouchRemote.Model.Persistence;
using TouchRemote.Model.Persistence.Controls;
using TouchRemote.UI.ConfigEditor;
using TouchRemote.UI.IconPicker;
using TouchRemote.Utils;
using WpfColorFontDialog;
using System.Windows.Media;

namespace TouchRemote.UI
{
    /// <summary>
    /// Interaction logic for ControlProperties.xaml
    /// </summary>
    internal partial class ControlProperties : MetroWindow
    {
        private IconManager _IconManager;

        public PluginManager PluginManager { get; private set; }

        public ICommand ConfigCommand { get; private set; }

        public ICommand OpenIconPickerCommand { get; private set; }

        public ICommand OpenFontPickerCommand { get; private set; }

        public ControlProperties(PluginManager pluginManager, IconManager iconManager, RemoteElement element)
        {
            PluginManager = pluginManager;
            _IconManager = iconManager;
            ConfigCommand = new DelegateCommand<object>(DoConfig);
            OpenIconPickerCommand = new DelegateCommand<IconHolder>(OpenIconPicker);
            OpenFontPickerCommand = new DelegateCommand<Font>(OpenFontPicker);
            InitializeComponent();
            propertyGrid.SelectedObject = element;
        }

        private void DoConfig(object configObject)
        {
            EditorWindow window = new EditorWindow(configObject);
            window.Owner = this;
            window.ShowDialog();
        }

        private void OpenIconPicker(IconHolder iconHolder)
        {
            PickerWindow window = new PickerWindow(iconHolder, _IconManager);
            window.Owner = this;
            window.ShowDialog();
        }

        private void OpenFontPicker(Font font)
        {
            ColorFontDialog fd = new ColorFontDialog(true, true, false);
            FontInfo fontInfo = new FontInfo(font.Family, font.Size, font.Style, FontStretches.Normal, font.Weight, Brushes.Black);
            fd.Font = fontInfo;
            var result = fd.ShowDialog();
            if (result.HasValue && result.Value)
            {
                font.Family = fd.Font.Family;
                font.Size = fd.Font.Size;
                font.Style = fd.Font.Style;
                font.Weight = fd.Font.Weight;
            }
        }
    }
}
