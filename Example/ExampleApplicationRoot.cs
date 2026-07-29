using System;

namespace RootPattern.Example
{
    /// <summary>
    /// Shared root used by both entry-point examples.
    /// </summary>
    public sealed class ExampleApplicationRoot : Root<ExampleRootContext>
    {
        private IDisposable _ownedResource;

        public ExampleApplicationRoot(ExampleRootContext context)
            : base(context)
        {
        }

        protected override void OnInitialize()
        {
            _ownedResource = new ExampleResource(Context.Log, Context.RootName);
            Context.Log.Write($"{Context.RootName}: initialized.");

            if (Context.View != null)
            {
                Context.View.Show($"{Context.RootName}: view dependency received from Unity.");
            }
        }

        protected override void OnDispose()
        {
            _ownedResource?.Dispose();
            _ownedResource = null;
            Context.Log.Write($"{Context.RootName}: disposed.");
        }

        private sealed class ExampleResource : IDisposable
        {
            private readonly IExampleLog _log;
            private readonly string _ownerName;

            public ExampleResource(IExampleLog log, string ownerName)
            {
                _log = log;
                _ownerName = ownerName;
            }

            public void Dispose()
            {
                _log.Write($"{_ownerName}: owned resource disposed.");
            }
        }
    }
}
