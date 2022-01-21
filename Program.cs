using System;
using System.IO;

namespace BlittableType
{
    class Program
    {
        private static FileHashCalculator _fileHashCalculator;

        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                PrintUsage();
                return;
            }

            var filePath = args[0];

            if (!File.Exists(filePath))
            {
                Console.WriteLine($"File {filePath} does not exist.");
                return;
            }

            if (!Int32.TryParse(args[1], out int blockSize))
            {
                Console.WriteLine($"Block size is in bad format: {args[1]}.");
                return;
            }

            Console.CancelKeyPress += new ConsoleCancelEventHandler(OnCancelKeyPress);

            using (_fileHashCalculator = new FileHashCalculator(filePath, blockSize))
            {
                _fileHashCalculator.Start();
            }
        }

        private static void PrintUsage()
        {
            Console.WriteLine("usage: dotnet file-hash.dll <FILE_PATH> <BLOCK_SIZE_BYTES>");
        }

        private static void OnCancelKeyPress(object? sender, ConsoleCancelEventArgs e)
        {
            _fileHashCalculator?.Stop();
            Console.WriteLine("Operation has been cancelled.");
        }
    }
}