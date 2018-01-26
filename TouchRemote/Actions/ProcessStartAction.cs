using System.Diagnostics;
using System.ComponentModel;
using TouchRemote.Lib;
using System.Xml;
using TouchRemote.Lib.Attributes;

namespace TouchRemote.Actions
{
    [DisplayName("Process Start")]
    [Description("Start a process or launch a URL")]
    public class ProcessStartAction : ActionExecutable
    {
        [ConfigProperty(Required = true)]
        [Category("Parameters")]
        [DisplayName("Start URL")]
        [Description("URL or file name to start, eg. \"notepad.exe\" or \"http://www.google.com\".")]
        [PropertyOrder(1)]
        public string StartUrl { get; set; }

        [ConfigProperty]
        [Category("Parameters")]
        [DisplayName("Command-line Arguments")]
        [Description("Command line arguments that will be passed to the process.")]
        [PropertyOrder(2)]
        public string Arguments { get; set; }

        public void Execute()
        {
            if (string.IsNullOrWhiteSpace(Arguments))
            {
                Process.Start(StartUrl);
            }
            else
            {
                Process.Start(StartUrl, Arguments);
            }
        }

        public void Serialize(XmlElement element)
        {
            element.SetAttribute("StartUrl", StartUrl);
            element.SetAttribute("Arguments", Arguments);
        }

        public void Deserialize(XmlElement element)
        {
            StartUrl = element.GetAttribute("StartUrl");
            Arguments = element.GetAttribute("Arguments");
        }
    }
}
