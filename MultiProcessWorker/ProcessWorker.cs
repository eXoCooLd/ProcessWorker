using MultiProcessWorker.Private.MultiProcessWorkerLogic;
using MultiProcessWorker.Public.Interfaces;
using System;

namespace MultiProcessWorker
{
    public static class ProcessWorker
    {
        internal static string RandomName => Guid.NewGuid().ToString("N");

        public static IMultiProcessWorker Create(string name = null)
        {
            return new MultiProcessWorkerClient(name ?? RandomName);
        }

        public static T RunAndWait<T>(Func<T> action) where T : class
        {
            using (var worker = Create())
            {
                return worker.ExecuteWait(action);
            }
        }
    }
}