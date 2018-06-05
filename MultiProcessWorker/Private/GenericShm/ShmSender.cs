#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// MIT License
// Copyright(c) 2018 Andre Wehrli

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// --------------------------------------------------------------------------------------------------------------------
#endregion Copyright

#region Used Namespaces
using System.IO.MemoryMappedFiles;
using System.Threading;
#endregion Used Namespaces

namespace MultiProcessWorker.Private.GenericShm
{
    /// <summary>
    /// Shared Memory Sender
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal abstract class ShmSender<T> : ShmBase
    {
        /// <summary>
        /// Current Buffer Size
        /// </summary>
        private long m_CurrentBufferSize;

        /// <summary>
        /// Shared Memory File
        /// </summary>
        private MemoryMappedFile m_MemoryFile;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="shmName"></param>
        protected ShmSender(string shmName) : base(shmName)
        {
        }

        /// <summary>
        /// Dispose
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_MemoryFile?.Dispose();
                m_MemoryFile = null;
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Save the data to the shared memory
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool SaveToSharedMemory(T data)
        {
            var dataBuffer = Serialize(data);
            var dataDoubleBufferSize = dataBuffer.LongLength * 2;

            var mutexName = CreateMutexName();
            bool createdNew;
            using (var shmMutex = new Mutex(true, mutexName, out createdNew))
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

        /// <summary>
        /// Write the dataBuffer to the shared memory
        /// </summary>
        /// <param name="dataBuffer"></param>
        private void WriteToSharedMemory(byte[] dataBuffer)
        {
            using (var accessor = m_MemoryFile.CreateViewAccessor(0, m_CurrentBufferSize))
            {
                var writeBuffer = new byte[m_CurrentBufferSize];
                dataBuffer.CopyTo(writeBuffer, 0);

                accessor.WriteArray(0, writeBuffer, 0, writeBuffer.Length);
            }
        }

        /// <summary>
        /// Resize the shared memory area if it is too small
        /// </summary>
        /// <param name="dataBuffer"></param>
        /// <param name="dataDoubleBufferSize"></param>
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

        /// <summary>
        /// Create the shared memory area if needed
        /// </summary>
        /// <param name="dataDoubleBufferSize"></param>
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

        /// <summary>
        /// Serialize the data into a byte array
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        protected abstract byte[] Serialize(T data);

    }
}