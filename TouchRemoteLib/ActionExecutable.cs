using System.Collections.Generic;

namespace TouchRemote.Lib
{
    /// <summary>
    /// Represents an action that can be executed as part of an event (click, toggleon, toggleoff)
    /// </summary>
    /// <remarks>
    /// Implementations will likely not be very useful without properties to describe their configuration. Therefore, implementations must provide a no-argument constructor, as well as serialize and deserialize methods, to allow the object's configuration properties to be persisted to and from storage. Implementations should use some sort of versioning on the serialized data to keep track of whether properties need to be added or removed.
    /// </remarks>
    public interface ActionExecutable : IXmlSerializable
    {
        /// <summary>
        /// This method is called in response to the event.
        /// </summary>
        /// <remarks>
        /// This method may be called on non-UI threads, background-worker threads, and multiple times on different threads. Implementations should not keep state, as the Execute method may be called multiple times for a given instance.
        /// </remarks>
        void Execute();
    }

    /// <summary>
    /// Represents a dynamic (provided by an ActionExecutableProvider) action
    /// </summary>
    public interface ProvidedActionExecutable : ActionExecutable, IProvided { }

    /// <summary>
    /// Allows plugins to provide a dynamic list of ActionExecutable implementations
    /// </summary>
    /// <example>
    /// A plugin could implement an ActionExecutable that activates (restores) a window, and a provider that returns a list of actions corresponding to the currently opened windows.
    /// </example>
    public interface ActionExecutableProvider
    {
        /// <summary>
        /// Create a list of provided implementations
        /// </summary>
        /// <returns>List of provided implementations</returns>
        IEnumerable<ActionExecutable> GetActions();
    }
}
