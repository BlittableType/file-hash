using System;

namespace BlittableType
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                PrintUsage();
                return;
            }
        }

        private static void PrintUsage()
        {
            Console.WriteLine("usage: dotnet file-hash.dll <FILE_PATH> <BLOCK_SIZE_BYTES>");
        }
    }
}