using System;

namespace MultiProcessWorker.Private.Ipc
{
    internal abstract class IpcBase : IDisposable
    {
        private const string ServerEventNamePrefix = @"Global\";
        private const string ServerEventNamePostfix = @"_Server";
        private const string ClientEventNamePrefix = @"Global\";
        private const string ClientEventNamePostfix = @"_Client";

        protected string IpcName { get; }

        protected IpcBase(string ipcName)
        {
            IpcName = ipcName;
        }

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

        protected string CreateEventNameServer()
        {
            return ServerEventNamePrefix + IpcName + ServerEventNamePostfix;
        }

        protected string CreateEventNameClient()
        {
            return ClientEventNamePrefix + IpcName + ClientEventNamePostfix;
        }

    }
}