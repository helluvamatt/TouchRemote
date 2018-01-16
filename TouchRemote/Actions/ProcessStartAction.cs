﻿using System.Diagnostics;
using System.ComponentModel;
using TouchRemote.Lib;
using System.Xml;

namespace TouchRemote.Actions
{
    [ActionName("Process Start")]
    [ActionDescription("Start a process or launch a URL")]
    public class ProcessStartAction : ActionExecutable
    {
        [ConfigProperty(Required = true)]
        [DisplayName("Start URL")]
        [Description("URL or file name to start, eg. \"notepad.exe\" or \"http://www.google.com\".")]
        public string StartUrl { get; set; }

        [ConfigProperty]
        [DisplayName("Command-line Arguments")]
        [Description("Command line arguments that will be passed to the process.")]
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
