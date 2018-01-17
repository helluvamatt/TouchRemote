using System.Collections.Generic;

namespace TouchRemote.Lib
{
    /// <summary>
    /// Represents a property or value that can be set as part of an event or fetched.
    /// </summary>
    /// <typeparam name="T">Type of the property</typeparam>
    /// <remarks>
    /// Generic implementations are not support and will not be created by the application. You must specify a concrete type parameter for <code>T</code>.
    /// </remarks>
    public interface BoundProperty<T> : IXmlSerializable
    {
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
    }

    /// <summary>
    /// Represents a dynamic (provided by a BoundPropertyProvider) bound property
    /// </summary>
    /// <typeparam name="T">Type of the property value</typeparam>
    public interface ProvidedBoundProperty<T> : BoundProperty<T>, IProvided { }

    /// <summary>
    /// Allows plugins to provide a dynamic list of <code>BoundProperty</code> implementations
    /// </summary>
    /// <typeparam name="T">Property type</typeparam>
    /// <remarks>
    /// You must specify the type of your BoundProperty implementation and the type of the property. Generic implementations are not supported.
    /// </remarks>
    public interface BoundPropertyProvider<TImpl, TProperty> where TImpl : ProvidedBoundProperty<TProperty>
    {
        /// <summary>
        /// Create a list of provided implementations
        /// </summary>
        /// <returns>List of provided implementations</returns>
        IEnumerable<TImpl> GetProperties();
    }
}
