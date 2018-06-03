using System;

namespace MultiProcessWorker.Public.Exceptions
{
    [Serializable]
    public class ProcessWorkerRemoteException : Exception
    {
        private const string MessageText = "Remote process exception: ";

        public ProcessWorkerRemoteException(Exception exception) : base(MessageText + exception, exception)
        {
            
        }
    }
}
