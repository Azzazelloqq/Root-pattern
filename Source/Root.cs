using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
#if PROJECT_SUPPORT_UNITASK
using RootTask = Cysharp.Threading.Tasks.UniTask;
#else
using RootTask = System.Threading.Tasks.Task;
#endif

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
        /// Asynchronously initializes the object graph composed by this root.
        /// </summary>
        /// <param name="token">Cancellation token to observe during initialization.</param>
        public async RootTask InitializeAsync(CancellationToken token)
        {
            EnsureCanInitialize();
            _state = RootState.Initializing;

            try
            {
                await OnInitializeAsync(token);

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
        /// Asynchronously creates and configures the object graph for this root.
        /// </summary>
        /// <param name="token">Cancellation token supplied to <see cref="InitializeAsync"/>.</param>
        protected abstract ValueTask OnInitializeAsync(CancellationToken token);

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
