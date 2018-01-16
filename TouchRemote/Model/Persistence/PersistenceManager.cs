using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace TouchRemote.Model.Persistence
{
    internal class PersistenceManager
    {
        private PluginManager _PluginManager;
        private string _XmlFilename;
        private XmlSchema _Schema;
        private ILog _Log;

        // TODO Serialization could be offloaded to another thread?
        private object _LockObj = new { };

        public PersistenceManager(PluginManager pluginManager, string xmlFilename)
        {
            _Log = LogManager.GetLogger(GetType());
            _PluginManager = pluginManager;
            _XmlFilename = xmlFilename;
            using (Stream schemaStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("TouchRemote.Model.Persistence.Buttons.xsd"))
            {
                _Schema = XmlSchema.Read(schemaStream, null);
            }
        }

        public void Save(IEnumerable<Model.Element> elements)
        {
            XmlDocument document = new XmlDocument();
            document.Schemas.Add(_Schema);
            List<Element> persistenceButtons = new List<Element>();
            foreach (var e in elements)
            {
                if (e is Model.ToggleButton)
                {
                    var toggleBtn = e as Model.ToggleButton;
                    ToggleButton persistenceBtn = new ToggleButton();
                    persistenceBtn.Id = toggleBtn.Id.ToString();
                    persistenceBtn.X = toggleBtn.X;
                    persistenceBtn.Y = toggleBtn.Y;
                    persistenceBtn.LabelOn = toggleBtn.LabelOn;
                    persistenceBtn.LabelOff = toggleBtn.LabelOff;
                    persistenceBtn.IconOn = toggleBtn.IconOn.ToString();
                    persistenceBtn.IconOff = toggleBtn.IconOff.ToString();
                    if (toggleBtn.ToggleOnAction != null && toggleBtn.ToggleOnActionImpl != null)
                    {
                        persistenceBtn.ToggleButtonToggleOnAction = new ActionExecutable();
                        persistenceBtn.ToggleButtonToggleOnAction.Type = toggleBtn.ToggleOnAction.Type.FullName;
                        persistenceBtn.ToggleButtonToggleOnAction.Any = document.CreateElement(toggleBtn.ToggleOnAction.Type.Name);
                        toggleBtn.ToggleOnActionImpl.Serialize(persistenceBtn.ToggleButtonToggleOnAction.Any);
                    }
                    if (toggleBtn.ToggleOffAction != null && toggleBtn.ToggleOffActionImpl != null)
                    {
                        persistenceBtn.ToggleButtonToggleOffAction = new ActionExecutable();
                        persistenceBtn.ToggleButtonToggleOffAction.Type = toggleBtn.ToggleOnAction.Type.FullName;
                        persistenceBtn.ToggleButtonToggleOffAction.Any = document.CreateElement(toggleBtn.ToggleOffAction.Type.Name);
                        toggleBtn.ToggleOnActionImpl.Serialize(persistenceBtn.ToggleButtonToggleOffAction.Any);
                    }
                    persistenceButtons.Add(persistenceBtn);
                }
                // Could possible have other button types here
                else if (e is Model.Button)
                {
                    var btn = e as Model.Button;
                    Button persistenceBtn = new Button();
                    persistenceBtn.Id = btn.Id.ToString();
                    persistenceBtn.X = btn.X;
                    persistenceBtn.Y = btn.Y;
                    persistenceBtn.Label = btn.Label;
                    persistenceBtn.Icon = btn.Icon.ToString();
                    if (btn.ClickAction != null && btn.ClickActionImpl != null)
                    {
                        persistenceBtn.ButtonClickAction = new ActionExecutable();
                        persistenceBtn.ButtonClickAction.Type = btn.ClickAction.Type.FullName;
                        persistenceBtn.ButtonClickAction.Any = document.CreateElement(btn.ClickAction.Type.Name);
                        btn.ClickActionImpl.Serialize(persistenceBtn.ButtonClickAction.Any);
                    }
                    persistenceButtons.Add(persistenceBtn);
                }
            }
            Buttons persistenceModel = new Buttons();
            persistenceModel.Items = persistenceButtons.ToArray();

            using (StreamWriter writer = new StreamWriter(File.Create(_XmlFilename)))
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                using (XmlWriter xmlWriter = XmlWriter.Create(writer, settings))
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(Buttons));
                    xmlSerializer.Serialize(xmlWriter, persistenceModel);
                }
            }
        }

        public IEnumerable<Model.Element> Load()
        {
            List<Model.Element> buttons = new List<Model.Element>();
            using (StreamReader reader = new StreamReader(File.OpenRead(_XmlFilename)))
            {
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.IgnoreComments = true;
                settings.IgnoreProcessingInstructions = true;
                settings.IgnoreWhitespace = true;
                settings.Schemas.Add(_Schema);
                settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessInlineSchema;
                settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessSchemaLocation;
                settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
                using (XmlReader xmlReader = XmlReader.Create(reader, settings))
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(Buttons));
                    Buttons deserialized = (Buttons)xmlSerializer.Deserialize(xmlReader);
                    foreach (Element b in deserialized.Items)
                    {
                        if (b is ToggleButton)
                        {
                            ToggleButton tb = b as ToggleButton;
                            Model.ToggleButton toggleButton = new Model.ToggleButton();
                            FontAwesome.WPF.FontAwesomeIcon iconOn = FontAwesome.WPF.FontAwesomeIcon.None;
                            Enum.TryParse(tb.IconOn, out iconOn);
                            toggleButton.IconOn = iconOn;
                            FontAwesome.WPF.FontAwesomeIcon iconOff = FontAwesome.WPF.FontAwesomeIcon.None;
                            Enum.TryParse(tb.IconOff, out iconOff);
                            toggleButton.IconOff = iconOff;
                            toggleButton.LabelOff = tb.LabelOff;
                            toggleButton.LabelOn = tb.LabelOn;
                            toggleButton.Id = new Guid(tb.Id);
                            toggleButton.X = tb.X;
                            toggleButton.Y = tb.Y;
                            if (tb.ToggleButtonToggleOffAction != null)
                            {
                                toggleButton.ToggleOffAction = _PluginManager.GetActionDescriptor(tb.ToggleButtonToggleOffAction.Type);
                                if (toggleButton.ToggleOffAction != null)
                                {
                                    toggleButton.ToggleOffActionImpl = _PluginManager.GetActionInstance(toggleButton.ToggleOffAction);
                                    toggleButton.ToggleOffActionImpl.Deserialize(tb.ToggleButtonToggleOffAction.Any);
                                }
                                else
                                {
                                    _Log.WarnFormat("Failed to find ActionExecutable of type \"{0}\" for \"ToggleButton.ToggleOffAction\", action will be \"(None)\"", tb.ToggleButtonToggleOffAction.Type);
                                }
                            }
                            if (tb.ToggleButtonToggleOnAction != null)
                            {
                                toggleButton.ToggleOnAction = _PluginManager.GetActionDescriptor(tb.ToggleButtonToggleOnAction.Type);
                                if (toggleButton.ToggleOnAction != null)
                                {
                                    toggleButton.ToggleOnActionImpl = _PluginManager.GetActionInstance(toggleButton.ToggleOnAction);
                                    toggleButton.ToggleOnActionImpl.Deserialize(tb.ToggleButtonToggleOnAction.Any);
                                }
                                else
                                {
                                    _Log.WarnFormat("Failed to find ActionExecutable of type \"{0}\" for \"ToggleButton.ToggleOnAction\", action will be \"(None)\"", tb.ToggleButtonToggleOnAction.Type);
                                }
                            }
                            buttons.Add(toggleButton);
                        }
                        else if (b is Button)
                        {
                            Button btn = b as Button;
                            var button = new Model.Button();
                            FontAwesome.WPF.FontAwesomeIcon icon = FontAwesome.WPF.FontAwesomeIcon.None;
                            Enum.TryParse(btn.Icon, out icon);
                            button.Icon = icon;
                            button.Label = btn.Label;
                            button.Id = new Guid(btn.Id);
                            button.X = btn.X;
                            button.Y = btn.Y;
                            if (btn.ButtonClickAction != null)
                            {
                                button.ClickAction = _PluginManager.GetActionDescriptor(btn.ButtonClickAction.Type);
                                if (button.ClickAction != null)
                                {
                                    button.ClickActionImpl = _PluginManager.GetActionInstance(button.ClickAction);
                                    button.ClickActionImpl.Deserialize(btn.ButtonClickAction.Any);
                                }
                                else
                                {
                                    _Log.WarnFormat("Failed to find ActionExecutable of type \"{0}\" for \"Button.ClickAction\", action will be \"(None)\"", btn.ButtonClickAction.Type);
                                }
                            }
                            buttons.Add(button);
                        }
                    }
                }
            }
            return buttons;
        }

    }
}
