using System;

namespace RootPattern
{
    /// <summary>
    /// Defines the explicit lifecycle of an application composition root.
    /// </summary>
    public interface IRoot : IDisposable
    {
        void Initialize();
    }
}
