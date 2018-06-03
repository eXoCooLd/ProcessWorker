using MultiProcessWorker.Public.ProcessData;
using System;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace MultiProcessWorker.Public.Exceptions
{
    [Serializable]
    public class ProcessWorkerCrashedException : Exception
    {
        private const string MessageText = "Remote process crashed";

        public Process Process { get; }
        public ExitCode ExitCode { get; }

        public ProcessWorkerCrashedException(Process process, ExitCode exitCode) : base(MessageText)
        {
            Process = process;
            ExitCode = exitCode;
        }

        protected ProcessWorkerCrashedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            Process = (Process)info.GetValue(nameof(Process), typeof(Process));
            ExitCode = (ExitCode)info.GetValue(nameof(ExitCode), typeof(ExitCode));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(Process), Process);
            info.AddValue(nameof(ExitCode), ExitCode);
        }
    }
}
