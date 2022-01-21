using System;

namespace BlittableType
{
    internal class ConsoleReporter : IProgress<string>
    {
        public void Report(string value)
        {
            Console.WriteLine(value);
        }
    }
}