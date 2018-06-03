using MultiProcessWorker.Private.JsonShm;
using System;
using System.Collections.Generic;
using System.Threading;

namespace MultiProcessWorker.Private.Ipc
{
    internal sealed class IpcReceiver<T> : IpcBase
    {
        private JsonShmReceiver<T> m_JsonShmReceiver;

        private Thread m_ReciveThread;
        private bool m_Running;

        public IpcReceiver(string ipcName) : base(ipcName)
        {
            ReciveQueue = new Queue<T>(10);

            m_JsonShmReceiver = new JsonShmReceiver<T>(IpcName);

            m_ReciveThread = new Thread(ReciveThreadMain);

            m_Running = true;
            m_ReciveThread.Start();
        }

        public Queue<T> ReciveQueue { get; }

        public event EventHandler DataRecived;

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