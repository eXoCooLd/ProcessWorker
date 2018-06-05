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

namespace MultiProcessWorker.Public.WorkItems
{
    /// <summary>
    /// Workjob result
    /// </summary>
    public class WorkResult : WorkCommand
    {
        /// <summary>
        /// Result of the workjob
        /// </summary>
        public object Result { get; set; }

        /// <summary>
        /// Exception in the remote workjob
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// Create a new WorkResult
        /// </summary>
        /// <param name="workCommand"></param>
        /// <param name="result"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Create a new WorkResult for a Exception
        /// </summary>
        /// <param name="workCommand"></param>
        /// <param name="exception"></param>
        /// <returns></returns>
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