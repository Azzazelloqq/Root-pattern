using UnityEngine;

namespace RootPattern.Example
{
    /// <summary>
    /// Demonstrates the Unity entry point. Add it to a scene or prefab and assign an ExampleView.
    /// </summary>
    public sealed class ExampleRootBehaviour : RootBehaviour
    {
        [SerializeField] private ExampleRootContext _context;

        private void Awake()
        {
            InitializeRoot();
        }

        protected override IRoot CreateRoot()
        {
            if (_context.View == null)
            {
                throw new MissingReferenceException("ExampleView must be assigned to ExampleRootBehaviour.");
            }

            return new ExampleApplicationRoot(_context.WithLog(new UnityExampleLog()));
        }

        private sealed class UnityExampleLog : IExampleLog
        {
            public void Write(string message)
            {
                Debug.Log(message);
            }
        }
    }
}
