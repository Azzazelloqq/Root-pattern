using System;

namespace RootPattern.Example
{
    /// <summary>
    /// Example application composition root.
    /// </summary>
    public sealed class ExampleApplicationRoot : Root
    {
        private readonly IExampleLog _log;
        private readonly string _rootName;
        private IDisposable _ownedResource;

        public ExampleApplicationRoot(IExampleLog log, string rootName)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _rootName = rootName ?? throw new ArgumentNullException(nameof(rootName));
        }

        protected override void OnInitialize()
        {
            _ownedResource = new ExampleResource(_log, _rootName);
            _log.Write($"{_rootName}: initialized.");
        }

        protected override void OnDispose()
        {
            _ownedResource?.Dispose();
            _ownedResource = null;
            _log.Write($"{_rootName}: disposed.");
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
