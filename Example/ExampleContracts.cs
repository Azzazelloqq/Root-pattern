namespace RootPattern.Example
{
    public interface IExampleLog
    {
        void Write(string message);
    }

    public interface IExampleView
    {
        void Show(string message);
    }

    public sealed class ExampleSettings
    {
        public ExampleSettings(string rootName)
        {
            RootName = rootName;
        }

        public string RootName { get; }
    }
}
