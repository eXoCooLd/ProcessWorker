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
using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Threading;
#endregion Used Namespaces

namespace MultiProcessWorker.Private.GenericShm
{
    /// <summary>
    /// Shared Memory Receiver
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal abstract class ShmReceiver<T> : ShmBase
    {

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="shmName"></param>
        protected ShmReceiver(string shmName) : base(shmName)
        {
        }

        /// <summary>
        /// Load the Data from the shared memory
        /// </summary>
        /// <returns></returns>
        public T LoadFromSharedMemory()
        {
            T data;

            var mutexName = CreateMutexName();
            bool createdNew;
            using (var shmMutex = new Mutex(true, mutexName, out createdNew))
            {
                if (!MutexIsFree(createdNew, shmMutex))
                {
                    return default(T);
                }

                data = ReadSharedMemory();

                shmMutex.ReleaseMutex();
            }

            return data;
        }

        /// <summary>
        /// Read from the shared memory
        /// </summary>
        /// <returns></returns>
        private T ReadSharedMemory()
        {
            var data = default(T);

            try
            {
                using (var shm = MemoryMappedFile.OpenExisting(SharedMemoryName, MemoryMappedFileRights.Read))
                {
                    using (var readStream = shm.CreateViewStream(0, 0, MemoryMappedFileAccess.Read))
                    {
                        var buffer = ReadAsByteArray(readStream);
                        data = Deserialize(buffer);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return data;
        }

        /// <summary>
        /// Read the stream as byte array
        /// </summary>
        /// <param name="dataStream"></param>
        /// <returns></returns>
        private static byte[] ReadAsByteArray(Stream dataStream)
        {
            byte[] data = null;

            try
            {
                var buffer = new byte[dataStream.Length];
                dataStream.Read(buffer, 0, buffer.Length);

                data = buffer;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return data;
        }

        /// <summary>
        /// Deserialize the bytte array to a object
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        protected abstract T Deserialize(byte[] data);

    }
}