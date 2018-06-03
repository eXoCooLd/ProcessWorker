using MultiProcessWorker.Public.EventArgs;
using System;

namespace MultiProcessWorker.Public.Interfaces
{
    public interface IMultiProcessWorker : IDisposable
    {
        event EventHandler<WorkCompleteEventArgs> WorkComplete;

        Guid Execute<T>(Func<T> action) where T : class;

        T ExecuteWait<T>(Func<T> action, long maxWait = -1) where T : class;

        bool IsDataReady(Guid guid);
        T GetResult<T>(Guid guid) where T : class;
    }
}