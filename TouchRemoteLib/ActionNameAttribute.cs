using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchRemote.Lib
{
    /// <summary>
    /// Attribute to apply a name to an ActionExecutable
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ActionNameAttribute : Attribute
    {
        public ActionNameAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }
    }
}
