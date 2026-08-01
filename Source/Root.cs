using System;
using System.Runtime.ExceptionServices;
using System.Threading;
namespace RootPattern
{
    /// <summary>
    /// Base class for an application composition root.
    /// </summary>
    public abstract class Root : IDisposable
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private RootState _state = RootState.Created;

        /// <summary>
        /// The current lifecycle state.
        /// </summary>
        public RootState State => _state;

        /// <summary>
        /// Is cancelled when this root begins disposal or initialization fails.
        /// </summary>
        public CancellationToken CancellationToken => _cancellationTokenSource.Token;

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
                _cancellationTokenSource.Cancel();
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
            Exception cancellationException = null;
            Exception disposeException = null;

            try
            {
                _cancellationTokenSource.Cancel();
            }
            catch (Exception exception)
            {
                cancellationException = exception;
            }

            try
            {
                OnDispose();
            }
            catch (Exception exception)
            {
                disposeException = exception;
            }
            finally
            {
                _cancellationTokenSource.Dispose();
                _state = RootState.Disposed;
            }

            if (cancellationException != null && disposeException != null)
            {
                throw new AggregateException(cancellationException, disposeException);
            }

            if (cancellationException != null)
            {
                ExceptionDispatchInfo.Capture(cancellationException).Throw();
            }

            if (disposeException != null)
            {
                ExceptionDispatchInfo.Capture(disposeException).Throw();
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
