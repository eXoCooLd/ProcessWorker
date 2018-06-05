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
#endregion Used Namespaces

namespace MultiProcessWorker.Private.Ipc
{
    internal abstract class IpcBase : IDisposable
    {

        #region Constants and Enums

        private const string ServerEventNamePrefix = @"Global\";
        private const string ServerEventNamePostfix = @"_Server";
        private const string ClientEventNamePrefix = @"Global\";
        private const string ClientEventNamePostfix = @"_Client";

        #endregion Constants and Enums

        #region Properties

        protected string IpcName { get; }

        #endregion Properties

        #region Constructor

        protected IpcBase(string ipcName)
        {
            IpcName = ipcName;
        }

        #endregion Constructor

        #region IDisposable Support

        protected virtual void Dispose(bool disposing)
        {
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~IpcBase()
        {
            Dispose(false);
        }

        #endregion IDisposable Support

        #region Protected

        protected string CreateEventNameServer()
        {
            return ServerEventNamePrefix + IpcName + ServerEventNamePostfix;
        }

        protected string CreateEventNameClient()
        {
            return ClientEventNamePrefix + IpcName + ClientEventNamePostfix;
        }

        #endregion Protected

    }
}