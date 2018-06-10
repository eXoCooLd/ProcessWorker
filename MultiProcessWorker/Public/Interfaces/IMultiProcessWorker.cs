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
        /// Remote hosted Object Type
        /// </summary>
        Type HostedObjecType { get; }

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
        Guid Execute<TResult>(Func<TResult> action);

        /// <summary>
        /// Execute new workjob ant return a new workId
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="action"></param>
        /// <param name="p1"></param>
        /// <returns></returns>
        Guid Execute<T1, TResult>(Func<T1, TResult> action, T1 p1);
        Guid Execute<T1, T2, TResult>(Func<T1, T2, TResult> action, T1 p1, T2 p2);
        Guid Execute<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> action, T1 p1, T2 p2, T3 p3);
        Guid Execute<T1, T2, T3, T4, TResult>(Func<T1, T2, T3, T4, TResult> action, T1 p1, T2 p2, T3 p3, T4 p4);
        Guid Execute<T1, T2, T3, T4, T5, TResult>(Func<T1, T2, T3, T4, T5, TResult> action, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5);
        Guid Execute<T1, T2, T3, T4, T5, T6, TResult>(Func<T1, T2, T3, T4, T5, T6, TResult> action, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6);
        Guid Execute<T1, T2, T3, T4, T5, T6, T7, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, TResult> action, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7);
        Guid Execute<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> action, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8);
        Guid Execute<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> action, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9);
        Guid Execute<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> action, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10);

        /// <summary>
        /// Execute new workjob and wait for the result
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="action"></param>
        /// <param name="maxWait"></param>
        /// <returns></returns>
        TResult ExecuteWait<TResult>(Func<TResult> action, long maxWait = -1);

        /// <summary>
        /// Execute new workjob and wait for the result
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <param name="action"></param>
        /// <param name="p1"></param>
        /// <param name="maxWait"></param>
        /// <returns></returns>
        TResult ExecuteWait<TResult, T1>(Func<T1, TResult> action, T1 p1, long maxWait = -1);
        TResult ExecuteWait<TResult, T1, T2>(Func<T1, T2, TResult> action, T1 p1, T2 p2, long maxWait = -1);
        TResult ExecuteWait<TResult, T1, T2, T3>(Func<T1, T2, T3, TResult> action, T1 p1, T2 p2, T3 p3, long maxWait = -1);
        TResult ExecuteWait<TResult, T1, T2, T3, T4>(Func<T1, T2, T3, T4, TResult> action, T1 p1, T2 p2, T3 p3, T4 p4, long maxWait = -1);
        TResult ExecuteWait<TResult, T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5, TResult> action, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, long maxWait = -1);
        TResult ExecuteWait<TResult, T1, T2, T3, T4, T5, T6>(Func<T1, T2, T3, T4, T5, T6, TResult> action, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, long maxWait = -1);
        TResult ExecuteWait<TResult, T1, T2, T3, T4, T5, T6, T7>(Func<T1, T2, T3, T4, T5, T6, T7, TResult> action, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, long maxWait = -1);
        TResult ExecuteWait<TResult, T1, T2, T3, T4, T5, T6, T7, T8>(Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> action, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, long maxWait = -1);
        TResult ExecuteWait<TResult, T1, T2, T3, T4, T5, T6, T7, T8, T9>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> action, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, long maxWait = -1);
        TResult ExecuteWait<TResult, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> action, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, long maxWait = -1);


        Guid Execute(Action action);
        Guid Execute<T1>(Action<T1> action, T1 p1);
        Guid Execute<T1, T2>(Action<T1, T2> action, T1 p1, T2 p2);
        Guid Execute<T1, T2, T3>(Action<T1, T2, T3> action, T1 p1, T2 p2, T3 p3);
        Guid Execute<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, T1 p1, T2 p2, T3 p3, T4 p4);
        Guid Execute<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> action, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5);
        Guid Execute<T1, T2, T3, T4, T5, T6>(Action<T1, T2, T3, T4, T5, T6> action, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6);
        Guid Execute<T1, T2, T3, T4, T5, T6, T7>(Action<T1, T2, T3, T4, T5, T6, T7> action, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7);
        Guid Execute<T1, T2, T3, T4, T5, T6, T7, T8>(Action<T1, T2, T3, T4, T5, T6, T7, T8> action, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8);
        Guid Execute<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> action, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9);
        Guid Execute<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> action, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10);

        void ExecuteWait(Action action, long maxWait = -1);
        void ExecuteWait<T1>(Action<T1> action, T1 p1, long maxWait = -1);
        void ExecuteWait<T1, T2>(Action<T1, T2> action, T1 p1, T2 p2, long maxWait = -1);
        void ExecuteWait<T1, T2, T3>(Action<T1, T2, T3> action, T1 p1, T2 p2, T3 p3, long maxWait = -1);
        void ExecuteWait<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, T1 p1, T2 p2, T3 p3, T4 p4, long maxWait = -1);
        void ExecuteWait<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> action, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, long maxWait = -1);
        void ExecuteWait<T1, T2, T3, T4, T5, T6>(Action<T1, T2, T3, T4, T5, T6> action, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, long maxWait = -1);
        void ExecuteWait<T1, T2, T3, T4, T5, T6, T7>(Action<T1, T2, T3, T4, T5, T6, T7> action, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, long maxWait = -1);
        void ExecuteWait<T1, T2, T3, T4, T5, T6, T7, T8>(Action<T1, T2, T3, T4, T5, T6, T7, T8> action, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, long maxWait = -1);
        void ExecuteWait<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> action, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, long maxWait = -1);
        void ExecuteWait<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> action, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, long maxWait = -1);
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
        TResult GetResult<TResult>(Guid guid);
    }
}