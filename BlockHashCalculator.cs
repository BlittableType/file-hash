using System.Security.Cryptography;

namespace BlittableType
{
    internal class BlockHashCalculator
    {
        private readonly SharedFile _file;
        private readonly int _blockSize;
        private readonly IProgress<string> _reporter;

        public BlockHashCalculator(SharedFile file, 
            int blockSize,
            IProgress<string> reporter)
        {
            _file = file;
            _blockSize = blockSize;
            _reporter = reporter;
        }

        public void DoCalculation(object cancellationTokenObj)
        {
            try
            {
                DoCalculationInternal((CancellationToken)cancellationTokenObj);
            }
            catch(Exception ex)
            {
                _reporter.Report($"[{Thread.CurrentThread.ManagedThreadId}] Error: {ex.Message}\r\n{ex.StackTrace}");
            }
        }

        private void DoCalculationInternal(CancellationToken cancellationToken)
        {
            byte[] block = new byte[_blockSize];
            ReadFileResult readResult;
            int bytesRead;
                        
            using (var hashFunc = SHA256.Create())
            {    
                while(!cancellationToken.IsCancellationRequested)
                {
                    // Read a block of data

                    readResult = _file.Read(block);
                    bytesRead = readResult.BytesRead;

                    if (bytesRead == 0)
                        break;

                    // Process last block
                    
                    if (bytesRead < _blockSize)
                    {
                        var lastBlock = new byte[bytesRead];
                        Buffer.BlockCopy(block, 0, lastBlock, 0, bytesRead);
                        CalcHashAndReportProgress(hashFunc, lastBlock, readResult.Reads);
                        break;
                    }

                    CalcHashAndReportProgress(hashFunc, block, readResult.Reads);
                }
            }
        }

        private void CalcHashAndReportProgress(HashAlgorithm hashFunc, byte[] block, long blockNum)
        {
            var hash = hashFunc.ComputeHash(block);
            _reporter.Report($"[{Thread.CurrentThread.ManagedThreadId}] {blockNum}:{BitConverter.ToString(hash)}");
        }
    }
}