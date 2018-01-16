using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchRemote.Lib
{
    /// <summary>
    /// Attribute to apply a description to an ActionExecutable type
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ActionDescriptionAttribute : Attribute
    {
        public ActionDescriptionAttribute(string description)
        {
            Description = description;
        }

        public string Description { get; private set; }
    }
}
