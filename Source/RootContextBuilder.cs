using System;
using System.Collections.Generic;

namespace RootPattern
{
    /// <summary>
    /// Builds an immutable context scope.
    /// </summary>
    public sealed class RootContextBuilder
    {
        private readonly IRootContext _parent;
        private readonly Dictionary<Type, object> _dependencies = new Dictionary<Type, object>();

        public RootContextBuilder(IRootContext parent = null)
        {
            _parent = parent;
        }

        /// <summary>
        /// Registers a dependency under its declared type.
        /// </summary>
        public RootContextBuilder Register<T>(T dependency)
        {
            if ((object)dependency == null)
            {
                throw new ArgumentNullException(nameof(dependency));
            }

            var dependencyType = typeof(T);
            if (_dependencies.ContainsKey(dependencyType))
            {
                throw new InvalidOperationException($"A dependency of type '{dependencyType.FullName}' is already registered in this scope.");
            }

            _dependencies.Add(dependencyType, dependency);
            return this;
        }

        /// <summary>
        /// Creates a context from the registered dependencies.
        /// </summary>
        public IRootContext Build()
        {
            return new RootContext(_parent, _dependencies);
        }
    }
}
