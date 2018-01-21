using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;

namespace TouchRemote.Web.Models
{
    public class WebControl
    {
        public WebControl(Guid id, int x, int y, int zIndex, WebControlType type, Dictionary<string, string> props)
        {
            Id = id.ToString();
            X = x;
            Y = y;
            ZIndex = zIndex;
            Type = type;
            Properties = props;
        }

        public string Id { get; private set; }

        public int X { get; private set; }

        public int Y { get; private set; }

        public int ZIndex { get; private set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public WebControlType Type { get; private set; }

        public Dictionary<string, string> Properties { get; private set; }

        public enum WebControlType
        {
            Unknown = 0,
            Button = 1,
            Slider = 2,
        }
    }
}
