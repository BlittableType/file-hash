using System.Threading;
using System.Security.Cryptography;

namespace BlittableType
{
    public class FileHashCalculator : IDisposable
    {
        private readonly string _filePath;
        private readonly int _blockSize;
        private readonly int _threadCount;
        private List<Thread> _calcThreads;
        private SharedFile _file;

        private int _blockNum;
        private int _blocks;
        private readonly object _syncRoot = new object();
        private CancellationTokenSource _cancelTokenSource;
        
        public FileHashCalculator(string filePath, int blockSize)
        {
            _filePath = filePath;
            _blockSize = blockSize;
            
            var file = new FileInfo(_filePath);
            _blocks = (int)Math.Ceiling((decimal)file.Length / _blockSize);

            _threadCount = Math.Min(_blocks, Environment.ProcessorCount);
            _calcThreads = new List<Thread>(_threadCount);

            Console.CancelKeyPress += new ConsoleCancelEventHandler(OnCancelKeyPress);
        }

        ~FileHashCalculator()
        {
            Dispose();
        }

        private void OnCancelKeyPress(object? sender, ConsoleCancelEventArgs e)
        {
            Stop();
            Console.WriteLine("Operation has been cancelled.");
        }

        public void Start()
        {
            ReopenFile();
            
            _calcThreads = new List<Thread>();
            _cancelTokenSource = new CancellationTokenSource();

            for (int i = 0; i < _threadCount; i++)
            {
                var thread = new Thread(new ParameterizedThreadStart(DoCalculation));
                _calcThreads.Add(thread);
                thread.Start(_cancelTokenSource.Token);
            }

            Console.WriteLine($"Threads: {_threadCount} ({String.Join(", ", _calcThreads.Select(t => t.ManagedThreadId))})");
            Console.WriteLine($"Blocks: {_blocks}");

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

        private void DoCalculation(object cancellationTokenObj)
        {
            var cancellationToken = (CancellationToken)cancellationTokenObj;
            byte[] block = new byte[_blockSize];
            long blockNum;
            int bytesRead;
                        
            using (var hashFunc = SHA256.Create())
            {    
                while(!cancellationToken.IsCancellationRequested)
                {
                    lock(_syncRoot)
                    {
                        blockNum = ++_blockNum;
                    }

                    // Read a block of data
                    bytesRead = _file.Read(block);

                    if (bytesRead == 0)
                        break;

                    // Process last block
                    
                    if (bytesRead < _blockSize)
                    {
                        var lastBlock = new byte[bytesRead];
                        Buffer.BlockCopy(block, 0, lastBlock, 0, bytesRead);
                        CalcAndDisplayHash(hashFunc, lastBlock, blockNum);
                        break;
                    }

                    CalcAndDisplayHash(hashFunc, block, blockNum);
                }
            }
        }

        private void CalcAndDisplayHash(HashAlgorithm hashFunc, byte[] block, long blockNum)
        {
            var hash = hashFunc.ComputeHash(block);
            Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}] {blockNum}:{BitConverter.ToString(hash)}");
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