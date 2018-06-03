using System;

namespace MultiProcessWorker.Public.WorkItems
{
    public class WorkCommand
    {
        public Guid Guid { get; set; }
        public Type Type { get; set; }
        public string Method { get; set; }

        public static WorkCommand Create(Type type, string method)
        {
            return new WorkCommand
                        {
                            Guid = Guid.NewGuid(),
                            Type = type,
                            Method = method
                        };
        }
    }
}