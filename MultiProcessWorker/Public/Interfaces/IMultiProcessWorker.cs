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
using MultiProcessWorker.Public.EventArgs;
using System;
#endregion Used Namespaces

namespace MultiProcessWorker.Public.Interfaces
{
    /// <summary>
    /// MultiProcessWorker Interface
    /// </summary>
    public interface IMultiProcessWorker : IDisposable
    {
        /// <summary>
        /// Work job completed event
        /// </summary>
        event EventHandler<WorkCompleteEventArgs> WorkComplete;

        /// <summary>
        /// Execute new workjob ant return a new workId
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="action"></param>
        /// <returns></returns>
        Guid Execute<TResult>(Func<TResult> action) where TResult : class;

        /// <summary>
        /// Execute new workjob ant return a new workId
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="action"></param>
        /// <param name="p1"></param>
        /// <returns></returns>
        Guid Execute<T1, TResult>(Func<T1, TResult> action, T1 p1) where TResult : class;

        /// <summary>
        /// Execute new workjob and wait for the result
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="action"></param>
        /// <param name="maxWait"></param>
        /// <returns></returns>
        TResult ExecuteWait<TResult>(Func<TResult> action, long maxWait = -1) where TResult : class;

        /// <summary>
        /// Execute new workjob and wait for the result
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <param name="action"></param>
        /// <param name="p1"></param>
        /// <param name="maxWait"></param>
        /// <returns></returns>
        TResult ExecuteWait<TResult, T1>(Func<T1, TResult> action, T1 p1, long maxWait = -1) where TResult : class;

        /// <summary>
        /// Check if the workjob is finished
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        bool IsDataReady(Guid guid);

        /// <summary>
        /// Get the result of a finished workjob
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="guid"></param>
        /// <returns></returns>
        TResult GetResult<TResult>(Guid guid) where TResult : class;
    }
}