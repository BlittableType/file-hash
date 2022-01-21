using System.IO;

namespace BlittableType
{
    internal class SharedFile : IDisposable
    {
        private readonly FileStream _file;
        private long _reads;

        public SharedFile(string filePath)
        {
            _file = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        }

        ~SharedFile()
        {
            Dispose();
        }

        public ReadFileResult Read(byte[] buffer)
        {
            lock(_file)
            {
                return new ReadFileResult(
                    bytesRead: _file.Read(buffer, 0, buffer.Length),
                    reads: ++_reads);
            }
        }
        
        private bool _isDisposed;

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
            _file.Dispose();
        }
    }
}