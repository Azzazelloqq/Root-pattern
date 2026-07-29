using System;

namespace RootPattern.Example
{
    /// <summary>
    /// Demonstrates creating an entry root without MonoBehaviour or Unity serialization.
    /// </summary>
    public static class PlainRootExample
    {
        public static Root Create()
        {
            var context = new RootContextBuilder()
                .Register<IExampleLog>(new ConsoleExampleLog())
                .Register(new ExampleSettings("Plain root"))
                .Build();

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
