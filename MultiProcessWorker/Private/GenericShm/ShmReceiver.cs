using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Threading;

namespace MultiProcessWorker.Private.GenericShm
{
    internal abstract class ShmReceiver<T> : ShmBase
    {
        protected ShmReceiver(string shmName) : base(shmName)
        {
        }

        public T LoadFromSharedMemory()
        {
            T data;

            var mutexName = CreateMutexName();
            using (var shmMutex = new Mutex(true, mutexName, out var createdNew))
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

        protected abstract T Deserialize(byte[] data);
    }
}