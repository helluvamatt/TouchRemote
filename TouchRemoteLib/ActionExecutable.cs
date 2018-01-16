using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace TouchRemote.Lib
{
    /// <summary>
    /// Represents an action that can be executed as part of an event (click, toggleon, toggleoff)
    /// </summary>
    /// <remarks>
    /// Implementations will likely not be very useful without properties to describe their configuration. Therefore, implementations must provide a no-argument constructor, as well as serialize and deserialize methods, to allow the object's configuration properties to be persisted to and from storage. Implementations should use some sort of versioning on the serialized data to keep track of whether properties need to be added or removed.
    /// </remarks>
    public interface ActionExecutable
    {
        /// <summary>
        /// This method is called in response to the event.
        /// </summary>
        /// <remarks>
        /// This method may be called on non-UI threads, background-worker threads, and multiple times on different threads. Implementations should not keep state, as the Execute method may be called multiple times for a given instance.
        /// </remarks>
        void Execute();

        /// <summary>
        /// Populate configuration properties from the given XmlElement
        /// </summary>
        /// <param name="element">XmlElement</param>
        void Deserialize(XmlElement element);

        /// <summary>
        /// Write the current instance's configuration properties to the given XmlElement
        /// </summary>
        /// <param name="element">XmlElement</param>
        void Serialize(XmlElement element);
    }
}
