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
using System.Collections.Generic;
using System.Threading;
#endregion Used Namespaces

namespace MultiProcessWorker.Private.Ipc
{
    /// <summary>
    /// Interprocess communication sender
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal sealed class IpcSender<T> : IpcBase
    {
        private readonly Queue<T> m_SendQueue;

        private JsonShmSender<T> m_JsonShmSender;
        private bool m_Running;
        private AutoResetEvent m_SendDataEvent;

        private Thread m_SendThread;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ipcName"></param>
        public IpcSender(string ipcName) : base(ipcName)
        {
            m_SendQueue = new Queue<T>(10);

            m_JsonShmSender = new JsonShmSender<T>(IpcName);

            m_SendThread = new Thread(SendThreadMain);
            m_SendDataEvent = new AutoResetEvent(false);

            m_Running = true;
            m_SendThread.Start();
        }

        /// <summary>
        /// Send Data
        /// </summary>
        /// <param name="data"></param>
        public void SendData(T data)
        {
            m_SendQueue.Enqueue(data);
            m_SendDataEvent.Set();
        }

        /// <summary>
        /// Wait until the send queue is empty
        /// </summary>
        public void WaitForAllSends()
        {
            while (m_SendQueue.Count > 0)
            {
                Thread.Sleep(1);
            }
        }

        /// <summary>
        /// Send Thread Main
        /// </summary>
        private void SendThreadMain()
        {
            var eventClientHandleName = CreateEventNameClient();
            var eventServerHandleName = CreateEventNameServer();

            using (var eventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, eventServerHandleName))
            {
                while (m_Running)
                {
                    if (m_SendDataEvent.WaitOne(0))
                    {
                        if (m_SendQueue.Count > 0)
                        {
                            SendData(eventWaitHandle, eventClientHandleName);
                        }
                        else
                        {
                            m_SendDataEvent.Reset();
                        }
                    }

                    Thread.Sleep(1);
                }
            }
        }

        /// <summary>
        /// Send Data
        /// </summary>
        /// <param name="eventWaitHandle"></param>
        /// <param name="eventClientHandleName"></param>
        private void SendData(EventWaitHandle eventWaitHandle, string eventClientHandleName)
        {
            var sendData = m_SendQueue.Peek();
            if (m_JsonShmSender.SaveToSharedMemory(sendData))
            {
                eventWaitHandle.Set();
                m_SendQueue.Dequeue();
                using (var readEventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, eventClientHandleName))
                {
                    readEventWaitHandle.WaitOne(1000);
                }
            }

            if (m_SendQueue.Count > 0)
            {
                m_SendDataEvent.Set();
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
                m_SendDataEvent?.Set();
                m_Running = false;

                m_SendThread?.Join();
                m_SendThread = null;

                m_JsonShmSender?.Dispose();
                m_JsonShmSender = null;

                m_SendDataEvent?.Dispose();
                m_SendDataEvent = null;
            }

            base.Dispose(disposing);
        }
    }
}