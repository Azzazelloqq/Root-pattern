namespace RootPattern.Example
{
    /// <summary>
    /// Demonstrates a child root that receives an overridden dependency from its scope.
    /// </summary>
    public sealed class ExampleFeatureRoot : Root
    {
        private readonly IExampleLog _log;
        private readonly ExampleSettings _settings;

        public ExampleFeatureRoot(IRootContext context)
            : base(context)
        {
            _log = context.Get<IExampleLog>();
            _settings = context.Get<ExampleSettings>();
        }

        protected override void OnInitialize()
        {
            _log.Write($"{_settings.RootName}: child initialized.");
        }

        protected override void OnDispose()
        {
            _log.Write($"{_settings.RootName}: child disposed.");
        }
    }
}
