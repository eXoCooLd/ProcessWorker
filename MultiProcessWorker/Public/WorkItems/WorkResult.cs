using System;

namespace MultiProcessWorker.Public.WorkItems
{
    public class WorkResult : WorkCommand
    {
        public object Result { get; set; }

        public Exception Exception { get; set; }

        public static WorkResult Create(WorkCommand workCommand, object result)
        {
            return new WorkResult
                        {
                            Guid = workCommand.Guid,
                            Type = workCommand.Type,
                            Method = workCommand.Method,
                            Result = result,
                            Exception = null
                        };
        }

        public static WorkResult Create(WorkCommand workCommand, Exception exception)
        {
            return new WorkResult
            {
                Guid = workCommand.Guid,
                Type = workCommand.Type,
                Method = workCommand.Method,
                Result = null,
                Exception = exception
            };
        }
    }
}