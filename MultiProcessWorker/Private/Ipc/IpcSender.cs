using MultiProcessWorker.Private.JsonShm;
using System.Collections.Generic;
using System.Threading;

namespace MultiProcessWorker.Private.Ipc
{
    internal sealed class IpcSender<T> : IpcBase
    {
        private readonly Queue<T> m_SendQueue;

        private JsonShmSender<T> m_JsonShmSender;
        private bool m_Running;
        private AutoResetEvent m_SendDataEvent;

        private Thread m_SendThread;

        public IpcSender(string ipcName) : base(ipcName)
        {
            m_SendQueue = new Queue<T>(10);

            m_JsonShmSender = new JsonShmSender<T>(IpcName);

            m_SendThread = new Thread(SendThreadMain);
            m_SendDataEvent = new AutoResetEvent(false);

            m_Running = true;
            m_SendThread.Start();
        }

        public void SendData(T data)
        {
            m_SendQueue.Enqueue(data);
            m_SendDataEvent.Set();
        }

        public void WaitForAllSends()
        {
            while (m_SendQueue.Count > 0)
            {
                Thread.Sleep(1);
            }
        }

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