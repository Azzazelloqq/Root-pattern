using System;
using System.Collections.Generic;

namespace RootPattern
{
    /// <summary>
    /// Base class for a node in a root tree.
    /// </summary>
    public abstract class Root : IDisposable
    {
        private readonly List<Root> _children = new List<Root>();
        private Root _parent;
        private RootState _state = RootState.Created;

        protected Root(IRootContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// The dependencies visible to this root.
        /// </summary>
        protected IRootContext Context { get; }

        /// <summary>
        /// The parent that owns this root, or <c>null</c> for an entry root.
        /// </summary>
        public Root Parent => _parent;

        /// <summary>
        /// The current lifecycle state.
        /// </summary>
        public RootState State => _state;

        /// <summary>
        /// Initializes this root and all children attached during initialization.
        /// </summary>
        public void Initialize()
        {
            EnsureCanInitialize();
            _state = RootState.Initializing;

            try
            {
                OnInitialize();

                for (var index = 0; index < _children.Count; index++)
                {
                    _children[index].Initialize();
                }

                _state = RootState.Initialized;
            }
            catch
            {
                _state = RootState.InitializationFailed;
                throw;
            }
        }

        /// <summary>
        /// Attaches a child root. The parent owns its lifecycle.
        /// </summary>
        protected TChild AddChild<TChild>(TChild child) where TChild : Root
        {
            if (child == null)
            {
                throw new ArgumentNullException(nameof(child));
            }

            if (_state == RootState.Disposing || _state == RootState.Disposed || _state == RootState.InitializationFailed)
            {
                throw new InvalidOperationException("A child cannot be added to a root that is no longer active.");
            }

            if (child._parent != null)
            {
                throw new InvalidOperationException("A root can only have one parent.");
            }

            child._parent = this;
            _children.Add(child);

            if (_state == RootState.Initialized)
            {
                child.Initialize();
            }

            return child;
        }

        /// <summary>
        /// Releases this root and its children. Disposal is safe to call repeatedly.
        /// </summary>
        public void Dispose()
        {
            if (_state == RootState.Disposed)
            {
                return;
            }

            if (_state == RootState.Disposing)
            {
                throw new InvalidOperationException("A root cannot dispose itself recursively.");
            }

            _state = RootState.Disposing;
            List<Exception> exceptions = null;

            for (var index = _children.Count - 1; index >= 0; index--)
            {
                TryDispose(_children[index], ref exceptions);
            }

            TryDisposeSelf(ref exceptions);
            _state = RootState.Disposed;

            if (exceptions != null)
            {
                throw new AggregateException(exceptions);
            }
        }

        /// <summary>
        /// Creates dependencies, resources and child roots for this root.
        /// </summary>
        protected abstract void OnInitialize();

        /// <summary>
        /// Releases resources owned directly by this root.
        /// </summary>
        protected virtual void OnDispose()
        {
        }

        private void EnsureCanInitialize()
        {
            if (_state != RootState.Created)
            {
                throw new InvalidOperationException($"A root in state '{_state}' cannot be initialized.");
            }

            if (_parent != null && _parent._state != RootState.Initializing && _parent._state != RootState.Initialized)
            {
                throw new InvalidOperationException("A child root is initialized only by its active parent.");
            }
        }

        private static void TryDispose(Root root, ref List<Exception> exceptions)
        {
            try
            {
                root.Dispose();
            }
            catch (Exception exception)
            {
                if (exceptions == null)
                {
                    exceptions = new List<Exception>();
                }

                exceptions.Add(exception);
            }
        }

        private void TryDisposeSelf(ref List<Exception> exceptions)
        {
            try
            {
                OnDispose();
            }
            catch (Exception exception)
            {
                if (exceptions == null)
                {
                    exceptions = new List<Exception>();
                }

                exceptions.Add(exception);
            }
        }
    }
}
