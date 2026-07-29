using System;

namespace RootPattern
{
    /// <summary>
    /// Provides dependencies available to a root and its descendants.
    /// </summary>
    public interface IRootContext
    {
        /// <summary>
        /// Gets the dependency registered for <typeparamref name="T"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">The dependency is not registered in this scope or a parent scope.</exception>
        T Get<T>();

        /// <summary>
        /// Tries to get the dependency registered for <typeparamref name="T"/>.
        /// </summary>
        bool TryGet<T>(out T dependency);

        /// <summary>
        /// Creates a child scope that can read this context and add its own dependencies.
        /// </summary>
        IRootContext CreateChild(Action<RootContextBuilder> configure = null);
    }
}
