using UnityEngine;

namespace RootPattern.Example
{
    /// <summary>
    /// Demonstrates the Unity entry point. Add it to a scene or prefab and assign an ExampleView.
    /// </summary>
    public sealed class ExampleRootBehaviour : RootBehaviour
    {
        [SerializeField] private ExampleView _view;
        [SerializeField] private string _rootName = "Scene root";

        private void Awake()
        {
            InitializeRoot();
        }

        protected override void ConfigureContext(RootContextBuilder builder)
        {
            if (_view == null)
            {
                throw new MissingReferenceException("ExampleView must be assigned to ExampleRootBehaviour.");
            }

            builder.Register<IExampleView>(_view);
            builder.Register<IExampleLog>(new UnityExampleLog());
            builder.Register(new ExampleSettings(_rootName));
        }

        protected override Root CreateRoot(IRootContext context)
        {
            return new ExampleApplicationRoot(context);
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
