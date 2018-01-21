using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace TouchRemote.Lib
{
    /// <summary>
    /// Represents a property or value that can be set as part of an event or fetched.
    /// </summary>
    /// <remarks>
    /// Generic implementations are not support and will not be created by the application. You must specify a concrete type parameter for <code>T</code>.
    /// </remarks>
    public interface FloatBoundProperty : IBoundProperty<float> { }

    /// <summary>
    /// Represents a dynamic (provided by a BoundPropertyProvider) bound property
    /// </summary>
    public interface ProvidedFloatBoundProperty : FloatBoundProperty, IProvided { }

    /// <summary>
    /// Allows plugins to provide a dynamic list of <code>BoundProperty</code> implementations
    /// </summary>
    /// <remarks>
    /// Implementations are shared as singletons inside the application
    /// </remarks>
    public interface FloatBoundPropertyProvider : IProvider<ProvidedFloatBoundProperty> { }
}
