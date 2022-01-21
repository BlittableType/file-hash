using System.IO;

namespace BlittableType
{
    public class SharedFile : IDisposable
    {
        private readonly FileStream _file;

        public SharedFile(string filePath)
        {
            _file = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        }

        ~SharedFile()
        {
            Dispose();
        }

        public int Read(byte[] buffer)
        {
            lock(_file)
            {
                return _file.Read(buffer, 0, buffer.Length);
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