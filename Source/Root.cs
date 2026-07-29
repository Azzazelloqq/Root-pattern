using System;
namespace RootPattern
{
    /// <summary>
    /// Base class for a strongly typed application composition root.
    /// </summary>
    public abstract class Root<TContext> : IRoot
        where TContext : struct, IRootContext
    {
        private RootState _state = RootState.Created;

        protected Root(TContext context)
        {
            Context = context;
        }

        /// <summary>
        /// Explicit dependencies required by this root.
        /// </summary>
        protected TContext Context { get; }

        /// <summary>
        /// The current lifecycle state.
        /// </summary>
        public RootState State => _state;

        /// <summary>
        /// Initializes the object graph composed by this root.
        /// </summary>
        public void Initialize()
        {
            EnsureCanInitialize();
            _state = RootState.Initializing;

            try
            {
                OnInitialize();

                _state = RootState.Initialized;
            }
            catch
            {
                _state = RootState.InitializationFailed;
                throw;
            }
        }

        /// <summary>
        /// Releases resources owned directly by this root. Disposal is safe to call repeatedly.
        /// </summary>
        public void Dispose()
        {
            if (_state == RootState.Disposed)
            {
                return;
            }

            if (_state == RootState.Disposing)
            {
                throw new InvalidOperationException("A root cannot be disposed while disposal is in progress.");
            }

            _state = RootState.Disposing;
            try
            {
                OnDispose();
            }
            finally
            {
                _state = RootState.Disposed;
            }
        }

        /// <summary>
        /// Creates and configures the object graph for this root.
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
        }
    }
}
