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
using System.Threading;
#endregion Used Namespaces

namespace MultiProcessWorker.Private.GenericShm
{
    /// <summary>
    /// Shared Memory Base Class
    /// </summary>
    internal abstract class ShmBase : IDisposable
    {

        #region Constants and Enums

        /// <summary>
        /// Null char for c style termination
        /// </summary>
        protected const char NullChar = '\0';

        /// <summary>
        /// Prefix for our mutex
        /// </summary>
        protected const string MutexPrefix = @"Global\";

        /// <summary>
        /// Postfix for our Mutex
        /// </summary>
        protected const string MutexPostfix = @"_mutex";

        #endregion Constants and Enums

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sharedMemoryName">Name of the shared memory</param>
        protected ShmBase(string sharedMemoryName)
        {
            SharedMemoryName = sharedMemoryName;
        }

        #endregion Constructor

        #region Properties

        /// <summary>
        /// Name of our SharedMemory
        /// </summary>
        protected string SharedMemoryName { get; }

        #endregion Properties

        #region Protected

        /// <summary>
        /// Create the Mutex Name
        /// </summary>
        /// <returns></returns>
        protected string CreateMutexName()
        {
            return MutexPrefix + SharedMemoryName + MutexPostfix;
        }

        /// <summary>
        /// Check if the Mutex is free
        /// </summary>
        /// <param name="createdNew"></param>
        /// <param name="mutex"></param>
        /// <returns></returns>
        protected static bool MutexIsFree(bool createdNew, Mutex mutex)
        {
            if (!createdNew)
            {
                var mutexIsFree = mutex.WaitOne(50);
                if (mutexIsFree == false) return false;
            }

            return true;
        }

        #endregion Protected

        #region IDisposable Support

        /// <summary>
        /// Dispose
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~ShmBase()
        {
            Dispose(false);
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support

    }
}