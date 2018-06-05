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
using MultiProcessWorker.Private.MultiProcessWorkerLogic;
using MultiProcessWorker.Public.Interfaces;
using System;
#endregion Used Namespaces

namespace MultiProcessWorker
{
    /// <summary>
    /// Process Worker
    /// </summary>
    public static class ProcessWorker
    {
        /// <summary>
        /// Generate a random name for the worker
        /// </summary>
        internal static string RandomName => Guid.NewGuid().ToString("N");

        /// <summary>
        /// Create a new ProcessWorker
        /// </summary>
        /// <param name="name"></param>
        /// <returns>New instance of a ProcessWorker</returns>
        public static IMultiProcessWorker Create(string name = null)
        {
            return new MultiProcessWorkerClient(name ?? RandomName);
        }

        /// <summary>
        /// Run a workjob with a new ProcessWorker and return the result
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="action"></param>
        /// <param name="maxWait"></param>
        /// <returns></returns>
        public static TResult RunAndWait<TResult>(Func<TResult> action, long maxWait = -1) where TResult : class
        {
            using (var worker = Create())
            {
                return worker.ExecuteWait(action);
            }
        }

        /// <summary>
        /// Run a workjob with a new ProcessWorker and return the result
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <param name="action"></param>
        /// <param name="p1"></param>
        /// <param name="maxWait"></param>
        /// <returns></returns>
        public static TResult RunAndWait<TResult, T1>(Func<T1, TResult> action, T1 p1, long maxWait = -1) where TResult : class
        {
            using (var worker = Create())
            {
                return worker.ExecuteWait(action, p1);
            }
        }
    }
}