using System;

namespace RootPattern.Example
{
    /// <summary>
    /// Demonstrates creating an entry root without MonoBehaviour or Unity serialization.
    /// </summary>
    public static class PlainRootExample
    {
        public static IRoot Create()
        {
            var context = new ExampleRootContext(
                view: null,
                rootName: "Plain root",
                log: new ConsoleExampleLog());

            return new ExampleApplicationRoot(context);
        }

        private sealed class ConsoleExampleLog : IExampleLog
        {
            public void Write(string message)
            {
                Console.WriteLine(message);
            }
        }
    }
}
