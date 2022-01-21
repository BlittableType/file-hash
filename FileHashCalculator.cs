using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace BlittableType
{
    internal class FileHashCalculator : IDisposable
    {
        private readonly string _filePath;
        private readonly int _blockSize;
        private readonly int _threadCount;
        private List<Thread> _calcThreads;
        private SharedFile _file;

        private int _blocks;
        private CancellationTokenSource _cancelTokenSource;

        private readonly IProgress<string> _reporter = new ConsoleReporter();
        
        public FileHashCalculator(string filePath, int blockSize)
        {
            _filePath = filePath;
            _blockSize = blockSize;
            
            var file = new FileInfo(_filePath);
            _blocks = (int)Math.Ceiling((decimal)file.Length / _blockSize);

            _threadCount = Math.Min(_blocks, Environment.ProcessorCount);
            _calcThreads = new List<Thread>(_threadCount);
        }

        ~FileHashCalculator()
        {
            Dispose();
        }

        public void Start()
        {
            ReopenFile();

            _calcThreads = new List<Thread>();
            _cancelTokenSource = new CancellationTokenSource();
            var calculator = new BlockHashCalculator(_file, _blockSize, _reporter);

            for (int i = 0; i < _threadCount; i++)
            {                
                var thread = new Thread(new ParameterizedThreadStart(calculator.DoCalculation));
                _calcThreads.Add(thread);
            }

            Console.WriteLine($"Threads: {_threadCount} ({String.Join(", ", _calcThreads.Select(t => t.ManagedThreadId))})");
            Console.WriteLine($"Blocks: {_blocks}");

            foreach (var thread in _calcThreads)
                thread.Start(_cancelTokenSource.Token);
            
            foreach (var thread in _calcThreads)
                thread.Join();
        }

        public void Stop()
        {
            _cancelTokenSource.Cancel();
            
            foreach (var thread in _calcThreads)
                thread.Join(5000);

            _file?.Dispose();
        }

        private void ReopenFile()
        {
            _file?.Dispose();
            _file = new SharedFile(_filePath);
        }

        private bool _isDisposed;

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            Stop();
            
            _calcThreads.Clear();
            _cancelTokenSource.Dispose();
        }
    }
}