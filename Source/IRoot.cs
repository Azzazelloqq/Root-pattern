using System;
using System.Threading;

namespace RootPattern
{
    /// <summary>
    /// Defines the explicit lifecycle of an application composition root.
    /// </summary>
    public interface IRoot : IDisposable
    {
        /// <summary>
        /// Is cancelled when the root begins disposal or initialization fails.
        /// </summary>
        CancellationToken CancellationToken { get; }

        void Initialize();
    }
}
