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
using MultiProcessWorker.Public.ProcessData;
using System;
using System.Diagnostics;
using System.Runtime.Serialization;
#endregion Used Namespaces

namespace MultiProcessWorker.Public.Exceptions
{
    /// <summary>
    /// ProcessWorker crash exception
    /// </summary>
    [Serializable]
    public class ProcessWorkerCrashedException : Exception
    {
        /// <summary>
        /// Exception message text
        /// </summary>
        private const string MessageText = "Remote process crashed";

        /// <summary>
        /// Remote worker process that crashed
        /// </summary>
        public Process Process { get; }

        /// <summary>
        /// ExitCode of the ProcessWorker
        /// </summary>
        public ExitCode ExitCode { get; }

        /// <summary>
        /// Exception Constructor
        /// </summary>
        /// <param name="process">Remote worker process</param>
        /// <param name="exitCode">ExitCode of the remote process</param>
        public ProcessWorkerCrashedException(Process process, ExitCode exitCode) : base(MessageText)
        {
            Process = process;
            ExitCode = exitCode;
        }

        /// <summary>
        /// Serialization Constructor for ISerializable
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected ProcessWorkerCrashedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            Process = (Process)info.GetValue(nameof(Process), typeof(Process));
            ExitCode = (ExitCode)info.GetValue(nameof(ExitCode), typeof(ExitCode));
        }

        /// <summary>
        /// Data Serializer for ISerializable 
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(Process), Process);
            info.AddValue(nameof(ExitCode), ExitCode);
        }
    }
}
