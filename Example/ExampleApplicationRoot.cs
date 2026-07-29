using System;

namespace RootPattern.Example
{
    /// <summary>
    /// Shared root used by both entry-point examples.
    /// </summary>
    public sealed class ExampleApplicationRoot : Root
    {
        private readonly IExampleLog _log;
        private readonly ExampleSettings _settings;
        private IDisposable _ownedResource;

        public ExampleApplicationRoot(IRootContext context)
            : base(context)
        {
            _log = context.Get<IExampleLog>();
            _settings = context.Get<ExampleSettings>();
        }

        protected override void OnInitialize()
        {
            _ownedResource = new ExampleResource(_log, _settings.RootName);
            _log.Write($"{_settings.RootName}: initialized.");

            var featureContext = Context.CreateChild(builder =>
                builder.Register(new ExampleSettings($"{_settings.RootName} feature")));

            AddChild(new ExampleFeatureRoot(featureContext));

            if (Context.TryGet<IExampleView>(out var view))
            {
                view.Show($"{_settings.RootName}: view dependency received from Unity.");
            }
        }

        protected override void OnDispose()
        {
            _ownedResource?.Dispose();
            _ownedResource = null;
            _log.Write($"{_settings.RootName}: disposed.");
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
