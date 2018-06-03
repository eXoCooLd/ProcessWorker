using System;

namespace MultiProcessWorker.Private.Ipc
{
    internal class IpcCommunication<T, TR> : IDisposable where TR : T
    {
        private IpcSender<T> m_IpcSender;
        private IpcReceiver<TR> m_IpcReceiver;

        public event EventHandler<TR> MessageRecived;

        public static IpcCommunication<T, TR> CreateServer(string serverName, string clientName)
        {
            return new IpcCommunication<T, TR>(serverName, clientName);
        }

        public static IpcCommunication<T, TR> CreateClient(string serverName, string clientName)
        {
            return new IpcCommunication<T, TR>(clientName, serverName);
        }

        private IpcCommunication(string senderName, string receiverName)
        {
            m_IpcSender = new IpcSender<T>(senderName);
            m_IpcReceiver = new IpcReceiver<TR>(receiverName);
            m_IpcReceiver.DataRecived += OnIpcDataRecived;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (m_IpcSender != null)
            {
                m_IpcSender.Dispose();
                m_IpcSender = null;
            }

            if (m_IpcReceiver != null)
            {
                m_IpcReceiver.DataRecived -= OnIpcDataRecived;
                m_IpcReceiver.Dispose();
                m_IpcReceiver = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~IpcCommunication()
        {
            Dispose(false);
        }

        public void SendData(T data)
        {
            m_IpcSender.SendData(data);
        }

        internal void WaitForAllSends()
        {
            m_IpcSender.WaitForAllSends();
        }

        internal void OnIpcDataRecived(object sender, EventArgs e)
        {
            while (m_IpcReceiver.ReciveQueue.Count > 0)
            {
                var resultItem = m_IpcReceiver.ReciveQueue.Dequeue();
                MessageRecived?.Invoke(this, resultItem);
            }
        }
    }
}
