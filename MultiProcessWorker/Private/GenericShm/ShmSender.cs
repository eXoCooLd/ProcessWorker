using System.IO.MemoryMappedFiles;
using System.Threading;

namespace MultiProcessWorker.Private.GenericShm
{
    internal abstract class ShmSender<T> : ShmBase
    {
        private long m_CurrentBufferSize;
        private MemoryMappedFile m_MemoryFile;

        protected ShmSender(string shmName) : base(shmName)
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_MemoryFile?.Dispose();
                m_MemoryFile = null;
            }

            base.Dispose(disposing);
        }

        public bool SaveToSharedMemory(T data)
        {
            var dataBuffer = Serialize(data);
            var dataDoubleBufferSize = dataBuffer.LongLength * 2;

            var mutexName = CreateMutexName();
            using (var shmMutex = new Mutex(true, mutexName, out var createdNew))
            {
                if (!MutexIsFree(createdNew, shmMutex))
                {
                    return false;
                }

                CreateSharedMemoryIfNeeded(dataDoubleBufferSize);

                if (m_MemoryFile == null)
                {
                    return false;
                }

                SetSharedMemorySize(dataBuffer, dataDoubleBufferSize);

                WriteToSharedMemory(dataBuffer);

                shmMutex.ReleaseMutex();
            }

            return true;
        }

        private void WriteToSharedMemory(byte[] dataBuffer)
        {
            using (var accessor = m_MemoryFile.CreateViewAccessor(0, m_CurrentBufferSize))
            {
                var writeBuffer = new byte[m_CurrentBufferSize];
                dataBuffer.CopyTo(writeBuffer, 0);

                accessor.WriteArray(0, writeBuffer, 0, writeBuffer.Length);
            }
        }

        private void SetSharedMemorySize(byte[] dataBuffer, long dataDoubleBufferSize)
        {
            if (m_CurrentBufferSize >= dataBuffer.Length)
            {
                return;
            }

            m_MemoryFile?.Dispose();
            m_MemoryFile = MemoryMappedFile.CreateNew(SharedMemoryName, dataDoubleBufferSize);
            m_CurrentBufferSize = dataDoubleBufferSize;
        }

        private void CreateSharedMemoryIfNeeded(long dataDoubleBufferSize)
        {
            if (m_MemoryFile == null || m_MemoryFile.SafeMemoryMappedFileHandle.IsClosed || m_MemoryFile.SafeMemoryMappedFileHandle.IsInvalid)
            {
                try
                {
                    m_MemoryFile = MemoryMappedFile.CreateNew(SharedMemoryName, dataDoubleBufferSize);
                    m_CurrentBufferSize = dataDoubleBufferSize;
                }
                catch
                {
                    m_MemoryFile = null;
                    m_CurrentBufferSize = 0;
                }
            }
        }

        protected abstract byte[] Serialize(T data);
    }
}