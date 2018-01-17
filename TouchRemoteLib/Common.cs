using System.Xml;

namespace TouchRemote.Lib
{
    /// <summary>
    /// Represents an object that can be serialized to and deserialized from XML
    /// </summary>
    public interface IXmlSerializable
    {
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

    /// <summary>
    /// Represents an implementation that is created by a provider implementation
    /// </summary>
    public interface IProvided
    {
        /// <summary>
        /// Display name of this provided implementation
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Description for this provided implementation
        /// </summary>
        string Description { get; }
    }
}
