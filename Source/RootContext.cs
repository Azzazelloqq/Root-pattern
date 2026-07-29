using System;
using System.Collections.Generic;

namespace RootPattern
{
    internal sealed class RootContext : IRootContext
    {
        private readonly IRootContext _parent;
        private readonly IReadOnlyDictionary<Type, object> _dependencies;

        public RootContext(IRootContext parent, IDictionary<Type, object> dependencies)
        {
            _parent = parent;
            _dependencies = new Dictionary<Type, object>(dependencies);
        }

        public T Get<T>()
        {
            if (TryGet<T>(out var dependency))
            {
                return dependency;
            }

            throw new InvalidOperationException($"A dependency of type '{typeof(T).FullName}' is not registered in this context.");
        }

        public bool TryGet<T>(out T dependency)
        {
            if (_dependencies.TryGetValue(typeof(T), out var value))
            {
                dependency = (T)value;
                return true;
            }

            if (_parent != null)
            {
                return _parent.TryGet(out dependency);
            }

            dependency = default;
            return false;
        }

        public IRootContext CreateChild(Action<RootContextBuilder> configure = null)
        {
            var builder = new RootContextBuilder(this);
            configure?.Invoke(builder);
            return builder.Build();
        }
    }
}
