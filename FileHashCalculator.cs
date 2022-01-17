using System.Threading;

namespace BlittableType
{
    public class FileHashCalculator
    {
        private readonly string _filePath;
        private readonly int _blockSize;
        private readonly int _threadCount;
        private readonly List<Thread> _calcThreads;

        private long _curPos;
        private readonly object _curPosSync = new object();
        private AutoResetEvent _cancelEvent;
        
        public FileHashCalculator(string filePath, int blockSize)
        {
            _filePath = filePath;
            _blockSize = blockSize;
            _threadCount = Environment.ProcessorCount;
            _calcThreads = new List<Thread>(_threadCount);
        }

        public void Start()
        {
            _cancelEvent = new AutoResetEvent(false);

            for (int i = 0; i < _threadCount; i++)
            {
                var thread = new Thread(new ThreadStart(DoCalculation));
                _calcThreads.Add(thread);
                thread.Start();
            }
        }

        public void Stop()
        {
            _cancelEvent.Set();
            
            foreach (var thread in _calcThreads)
                thread.Join(5000);

            _calcThreads.Clear();
            _cancelEvent.Close();
        }

        private void DoCalculation()
        {
            using (var file = new FileStream(_filePath, FileMode.Open, FileAccess.Read))
            {
                byte[] block = new byte[_blockSize];

                while(!_cancelEvent.WaitOne())
                {
                    lock(_curPosSync)
                    {
                        _curPos += _blockSize;
                    }

                    file.Seek(_curPos, SeekOrigin.Begin);                    
                    file.Read(block, 0, _blockSize);

                    // TODO: calculate hash of block
                    // TODO: display hash value
                }
            }
        }
    }
}