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
    /// <summary>
    /// Interprocess Communication
    /// </summary>
    /// <typeparam name="T">WorkCommand</typeparam>
    /// <typeparam name="TR">WorkResult</typeparam>
    internal class IpcCommunication<T, TR> : IDisposable where TR : T
    {

        #region Fields

        private IpcSender<T> m_IpcSender;
        private IpcReceiver<TR> m_IpcReceiver;

        #endregion Fields

        #region Events

        public event EventHandler<TR> MessageRecived;

        #endregion Events

        #region Public Factories

        public static IpcCommunication<T, TR> CreateServer(string serverName, string clientName)
        {
            return new IpcCommunication<T, TR>(serverName, clientName);
        }

        public static IpcCommunication<T, TR> CreateClient(string serverName, string clientName)
        {
            return new IpcCommunication<T, TR>(clientName, serverName);
        }

        #endregion Public Factories

        #region Constructor

        private IpcCommunication(string senderName, string receiverName)
        {
            m_IpcSender = new IpcSender<T>(senderName);
            m_IpcReceiver = new IpcReceiver<TR>(receiverName);
            m_IpcReceiver.DataRecived += OnIpcDataRecived;
        }

        #endregion Constructor

        #region IDisposable Support

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

        #endregion IDisposable Support

        #region Public

        public void SendData(T data)
        {
            m_IpcSender.SendData(data);
        }

        #endregion Public

        #region Internal

        internal void WaitForAllSends()
        {
            m_IpcSender.WaitForAllSends();
        }

        #endregion Internal

        #region Private

        private void OnIpcDataRecived(object sender, EventArgs e)
        {
            while (m_IpcReceiver.ReciveQueue.Count > 0)
            {
                var resultItem = m_IpcReceiver.ReciveQueue.Dequeue();
                MessageRecived?.Invoke(this, resultItem);
            }
        }

        #endregion Private
    }
}
