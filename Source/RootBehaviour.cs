using System;
using UnityEngine;

namespace RootPattern
{
    /// <summary>
    /// Unity adapter that creates a plain C# root from serialized scene or prefab references.
    /// </summary>
    public abstract class RootBehaviour : MonoBehaviour
    {
        private IRoot _root;
        private bool _disposed;

        /// <summary>
        /// The created root. It is available after <see cref="InitializeRoot"/> has been called.
        /// </summary>
        public IRoot Root => _root ?? throw new InvalidOperationException("The root has not been created yet.");

        /// <summary>
        /// Builds the context, creates the root if needed and initializes it.
        /// </summary>
        public void InitializeRoot()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }

            if (_root == null)
            {
                _root = CreateRoot() ?? throw new InvalidOperationException("CreateRoot returned null.");
            }

            _root.Initialize();
        }

        /// <summary>
        /// Releases the created root. It can be called before Unity destroys this component.
        /// </summary>
        public void DisposeRoot()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _root?.Dispose();
        }

        /// <summary>
        /// Creates the plain C# entry root for this component.
        /// </summary>
        protected abstract IRoot CreateRoot();

        protected virtual void OnDestroy()
        {
            DisposeRoot();
        }
    }
}
