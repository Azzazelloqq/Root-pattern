using System;

namespace RootPattern.Example
{
    /// <summary>
    /// Demonstrates creating an entry root with explicit constructor dependencies.
    /// </summary>
    public static class PlainRootExample
    {
        public static Root Create()
        {
            return new ExampleApplicationRoot(new ConsoleExampleLog(), "Plain root");
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
