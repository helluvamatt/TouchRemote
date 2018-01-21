using System;
using System.Collections.Generic;
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

    /// <summary>
    /// Provides instances of T
    /// </summary>
    /// <typeparam name="T">Type of item to provide</typeparam>
    public interface IProvider<T>
    {
        /// <summary>
        /// Get an empty default instance that will be populated by the application
        /// </summary>
        T EmptyInstance { get; }

        /// <summary>
        /// Create a list of provided implementations
        /// </summary>
        /// <returns>List of provided implementations</returns>
        IEnumerable<T> GetProperties();
    }

    /// <summary>
    /// Common interface for bound properties
    /// </summary>
    public interface IBoundProperty<T> : IXmlSerializable, IDisposable
    {
        /// <summary>
        /// Initialize any resources needed: streams, devices, etc.
        /// </summary>
        /// <returns>True if successful, false otherwise</returns>
        bool Initialize();

        /// <summary>
        /// This method is called in response to an event to set the property to the given value
        /// </summary>
        /// <param name="parameter">New value of the property</param>
        void SetValue(T parameter);

        /// <summary>
        /// This method is called to fetch the current value. It may be called at regular intervals and on different threads.
        /// </summary>
        /// <returns>current value of the property</returns>
        T GetValue();

        /// <summary>
        /// Does this bound property support using the ValueChanged event to notify the applicaton of changes
        /// </summary>
        bool SupportsValueChanged { get; }

        /// <summary>
        /// Event that the application will listen for value changes when SupportsValueChanged is true
        /// </summary>
        event BoundPropertyValueChangedHandler<T> ValueChanged;
    }

    /// <summary>
    /// Delegate for the BoundProperty.ValueChanged event
    /// </summary>
    /// <param name="sender">The BoundProperty that changed</param>
    /// <param name="args">Event args</param>
    public delegate void BoundPropertyValueChangedHandler<T>(object sender, BoundPropertyValueChangedEventArgs<T> args);

    /// <summary>
    /// EventArgs for the BoundProperty.ValueChanged event
    /// </summary>
    public class BoundPropertyValueChangedEventArgs<T> : EventArgs
    {
        /// <summary>
        /// C'tor
        /// </summary>
        /// <param name="oldValue">OldValue property</param>
        /// <param name="newValue">NewValue property</param>
        public BoundPropertyValueChangedEventArgs(T oldValue, T newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        /// <summary>
        /// The old value
        /// </summary>
        public T OldValue { get; private set; }

        /// <summary>
        /// The new value
        /// </summary>
        public T NewValue { get; private set; }
    }
}
