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
using MultiProcessWorker.Private.JsonShm;
using System;
using System.Collections.Generic;
using System.Threading;
#endregion Used Namespaces

namespace MultiProcessWorker.Private.Ipc
{
    /// <summary>
    /// Interprocess communication reciver
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal sealed class IpcReceiver<T> : IpcBase
    {
        private JsonShmReceiver<T> m_JsonShmReceiver;

        private Thread m_ReciveThread;
        private bool m_Running;

        public Queue<T> ReciveQueue { get; }

        public event EventHandler DataRecived;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ipcName"></param>
        public IpcReceiver(string ipcName) : base(ipcName)
        {
            ReciveQueue = new Queue<T>(10);

            m_JsonShmReceiver = new JsonShmReceiver<T>(IpcName);

            m_ReciveThread = new Thread(ReciveThreadMain);

            m_Running = true;
            m_ReciveThread.Start();
        }

        /// <summary>
        /// Recive Thread Main
        /// </summary>
        private void ReciveThreadMain()
        {
            var eventClientHandleName = CreateEventNameClient();
            var eventServerHandleName = CreateEventNameServer();
            using (var eventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, eventServerHandleName))
            {
                while (m_Running)
                {
                    if (eventWaitHandle.WaitOne(1))
                    {
                        ReadData(eventClientHandleName);
                    }

                    Thread.Sleep(1);
                }
            }
        }

        /// <summary>
        /// Read Data
        /// </summary>
        /// <param name="eventClientHandleName"></param>
        private void ReadData(string eventClientHandleName)
        {
            using (var readEventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, eventClientHandleName))
            {
                var data = m_JsonShmReceiver.LoadFromSharedMemory();
                if (data != null)
                {
                    ReciveQueue.Enqueue(data);

                    DataRecived?.Invoke(this, EventArgs.Empty);
                }

                readEventWaitHandle.Set();
            }
        }

        /// <summary>
        /// Dispose
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_Running = false;

                m_ReciveThread?.Join();
                m_ReciveThread = null;

                m_JsonShmReceiver?.Dispose();
                m_JsonShmReceiver = null;
            }

            base.Dispose(disposing);
        }
    }
}