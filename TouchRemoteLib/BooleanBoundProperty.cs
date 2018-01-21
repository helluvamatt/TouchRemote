using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace TouchRemote.Lib
{
    /// <summary>
    /// Represents a property or value that can be set as part of an event or fetched.
    /// </summary>
    public interface BooleanBoundProperty : IBoundProperty<bool> { }

    /// <summary>
    /// Represents a dynamic (provided by a BoundPropertyProvider) bound property
    /// </summary>
    public interface ProvidedBooleanBoundProperty : BooleanBoundProperty, IProvided { }

    /// <summary>
    /// Allows plugins to provide a dynamic list of <code>BoundProperty</code> implementations
    /// </summary>
    /// <remarks>
    /// Implementations are shared as singletons inside the application
    /// </remarks>
    public interface BooleanBoundPropertyProvider : IProvider<ProvidedBooleanBoundProperty> { }
}
