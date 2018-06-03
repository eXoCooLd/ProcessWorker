using System;
using System.Threading;

namespace MultiProcessWorker.Private.GenericShm
{
    internal abstract class ShmBase : IDisposable
    {
        protected const char NullChar = '\0';

        protected const string MutexPrefix = @"Global\";
        protected const string MutexPostfix = @"_mutex";

        protected ShmBase(string sharedMemoryName)
        {
            SharedMemoryName = sharedMemoryName;
        }

        protected string SharedMemoryName { get; }

        protected string CreateMutexName()
        {
            return MutexPrefix + SharedMemoryName + MutexPostfix;
        }

        protected static bool MutexIsFree(bool createdNew, Mutex mutex)
        {
            if (!createdNew)
            {
                var mutexIsFree = mutex.WaitOne(50);
                if (mutexIsFree == false) return false;
            }

            return true;
        }

        #region IDisposable Support

        protected virtual void Dispose(bool disposing)
        {
        }

        ~ShmBase()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}