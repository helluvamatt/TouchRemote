using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;

namespace TouchRemote.Web.Models
{
    public class WebControl
    {
        public WebControl(Guid id, WebControlType type, Dictionary<string, string> style, Dictionary<string, string> props)
        {
            Id = id.ToString();
            Type = type;
            Styles = style;
            Properties = props;
        }

        public string Id { get; private set; }

        public Dictionary<string, string> Styles { get; private set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public WebControlType Type { get; private set; }

        public Dictionary<string, string> Properties { get; private set; }

        public enum WebControlType
        {
            Unknown = 0,
            Button = 1,
            Slider = 2,
            Label = 3,
        }
    }
}
