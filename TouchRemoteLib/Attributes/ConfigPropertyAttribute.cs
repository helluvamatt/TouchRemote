using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchRemote.Lib.Attributes
{
    /// <summary>
    /// Mark a property as a configuration property for the ActionExecutable
    /// </summary>
    /// <remarks>
    /// <para>This attribute is used to determine which properties on an implementation instance of ActionExecutable will be shown to the user to be edited.</para>
    /// <para>Properties marked "Required" will add validation to certain editors:</para>
    /// <list type="string">
    /// <item>Primitives, struct and Nullable: No useful effect, the default value of the primitive is always populated, Nullable will always have a value and may be confusing for the user</item>
    /// <item>String, Uri, FileInfo: Will check that the string or Uri is set and not empty, FileInfo will check if the file exists and is at least readable by the application</item>
    /// </list>
    /// <para>Most properties will be string, but the editor supports:</para>
    /// <list type="string">
    /// <item>Primitives such as int, bool, double, or string</item>
    /// <item>System.DateTime (using a date picker)</item>
    /// <item>System.Drawing.Color (using a color picker)</item>
    /// <item>System.TimeSpan</item>
    /// <item>System.Uri (uses a text box for editing, except it adds validation for being a valid Uri)</item>
    /// <item>System.IO.FileInfo (used as a wrapper around a path string, uses a File picker editor)</item>
    /// <item>System.Enum (Enums must be publicly exported by your plugin library, uses a dropdown control to choose values)</item>
    /// <item>System.TimeZoneInfo (Uses a combo box populated by the known timezones read from the OS)</item>
    /// <item>Nullable of any of the above primitive or struct types (adds a "(None)" option to editors)</item>
    /// </list>
    /// <para>Other types are not supported and will be ignored by the editor UI.</para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property)]
    public class ConfigPropertyAttribute : Attribute
    {
        public bool Required { get; set; }

        public ConfigPropertyAttribute(bool required)
        {
            Required = required;
        }

        public ConfigPropertyAttribute()
        {
            Required = false;
        }
    }
}
