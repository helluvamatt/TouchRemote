using FontAwesome.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchRemote.Web.Models
{
    public class WebButton
    {
        public WebButton(Guid id, string label, FontAwesomeIcon icon)
        {
            Id = id.ToString();
            Label = label;
            var conv = new FontAwesome.WPF.Converters.CssClassNameConverter() { Mode = FontAwesome.WPF.Converters.CssClassConverterMode.FromIconToString };
            IconClass = (string)conv.Convert(icon, typeof(string), null, null);
        }

        public string Id { get; private set; }

        public string IconClass { get; private set; }

        public string Label { get; private set; }
    }
}
