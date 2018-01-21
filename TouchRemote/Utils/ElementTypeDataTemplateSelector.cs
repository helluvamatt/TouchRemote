using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace TouchRemote.Utils
{
    [ContentProperty("Choices")]
    public class ElementTypeDataTemplateSelector : DataTemplateSelector
    {
        public List<ElementTypeDataTemplateSelectorTemplate> Choices { get; set; }

        public DataTemplate DefaultTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            foreach (var choice in Choices)
            {
                if (item.GetType().Name == choice.Value)
                {
                    return choice.Template;
                }
            }
            return DefaultTemplate;
        }
    }

    [ContentProperty("Template")]
    public class ElementTypeDataTemplateSelectorTemplate
    {
        public DataTemplate Template { get; set; }

        public string Value { get; set; }
    }
}
