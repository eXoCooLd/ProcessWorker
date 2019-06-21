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

        #region Internal

        /// <summary>
        /// Generate a random name for the worker
        /// </summary>
        internal static string RandomName => Guid.NewGuid().ToString("N");

        #endregion Internal

        #region Create ProcessWorker Instance

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
        /// Create a remote worker with a hosted object
        /// </summary>
        /// <param name="name"></param>
        /// <param name="maxShutdownTimeout"></param>
        /// <returns></returns>
        public static IMultiProcessWorker Create<T>(string name = null, int maxShutdownTimeout = 60000)
        {
            return new MultiProcessWorkerClient(name ?? RandomName, maxShutdownTimeout, typeof(T));
        }

        #endregion Create ProcessWorker Instance

        #region RunAndWait with Func

        /// <summary>
        /// Run a workjob with a new ProcessWorker and return the result
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="action"></param>
        /// <param name="maxWait"></param>
        /// <returns></returns>
        public static TResult RunAndWait<TResult>(Func<TResult> action, long maxWait = -1)
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
        public static TResult RunAndWait<TResult, T1>(Func<T1, TResult> action, T1 p1, long maxWait = -1)
        {
            using (var worker = Create())
            {
                return worker.ExecuteWait(action, p1, maxWait);
            }
        }

        /// <summary>
        /// Run a workjob with a new ProcessWorker and return the result
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="action"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="maxWait"></param>
        /// <returns></returns>
        public static TResult RunAndWait<TResult, T1, T2>(Func<T1, T2, TResult> action, T1 p1, T2 p2, long maxWait = -1)
        {
            using (var worker = Create())
            {
                return worker.ExecuteWait(action, p1, p2, maxWait);
            }
        }

        /// <summary>
        /// Run a workjob with a new ProcessWorker and return the result
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <param name="action"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <param name="maxWait"></param>
        /// <returns></returns>
        public static TResult RunAndWait<TResult, T1, T2, T3>(Func<T1, T2, T3, TResult> action, T1 p1, T2 p2, T3 p3, long maxWait = -1)
        {
            using (var worker = Create())
            {
                return worker.ExecuteWait(action, p1, p2, p3, maxWait);
            }
        }

        /// <summary>
        /// Run a workjob with a new ProcessWorker and return the result
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <param name="action"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <param name="p4"></param>
        /// <param name="maxWait"></param>
        /// <returns></returns>
        public static TResult RunAndWait<TResult, T1, T2, T3, T4>(Func<T1, T2, T3, T4, TResult> action, T1 p1, T2 p2, T3 p3, T4 p4, long maxWait = -1)
        {
            using (var worker = Create())
            {
                return worker.ExecuteWait(action, p1, p2, p3, p4, maxWait);
            }
        }

        /// <summary>
        /// Run a workjob with a new ProcessWorker and return the result
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <param name="action"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <param name="p4"></param>
        /// <param name="p5"></param>
        /// <param name="maxWait"></param>
        /// <returns></returns>
        public static TResult RunAndWait<TResult, T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5, TResult> action, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, long maxWait = -1)
        {
            using (var worker = Create())
            {
                return worker.ExecuteWait(action, p1, p2, p3, p4, p5, maxWait);
            }
        }

        /// <summary>
        /// Run a workjob with a new ProcessWorker and return the result
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <param name="action"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <param name="p4"></param>
        /// <param name="p5"></param>
        /// <param name="p6"></param>
        /// <param name="maxWait"></param>
        /// <returns></returns>
        public static TResult RunAndWait<TResult, T1, T2, T3, T4, T5, T6>(Func<T1, T2, T3, T4, T5, T6, TResult> action, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, long maxWait = -1)
        {
            using (var worker = Create())
            {
                return worker.ExecuteWait(action, p1, p2, p3, p4, p5, p6, maxWait);
            }
        }

        /// <summary>
        /// Run a workjob with a new ProcessWorker and return the result
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <param name="action"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <param name="p4"></param>
        /// <param name="p5"></param>
        /// <param name="p6"></param>
        /// <param name="p7"></param>
        /// <param name="maxWait"></param>
        /// <returns></returns>
        public static TResult RunAndWait<TResult, T1, T2, T3, T4, T5, T6, T7>(Func<T1, T2, T3, T4, T5, T6, T7, TResult> action, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, long maxWait = -1)
        {
            using (var worker = Create())
            {
                return worker.ExecuteWait(action, p1, p2, p3, p4, p5, p6, p7, maxWait);
            }
        }

        /// <summary>
        /// Run a workjob with a new ProcessWorker and return the result
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <param name="action"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <param name="p4"></param>
        /// <param name="p5"></param>
        /// <param name="p6"></param>
        /// <param name="p7"></param>
        /// <param name="p8"></param>
        /// <param name="maxWait"></param>
        /// <returns></returns>
        public static TResult RunAndWait<TResult, T1, T2, T3, T4, T5, T6, T7, T8>(Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> action, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, long maxWait = -1)
        {
            using (var worker = Create())
            {
                return worker.ExecuteWait(action, p1, p2, p3, p4, p5, p6, p7, p8, maxWait);
            }
        }

        /// <summary>
        /// Run a workjob with a new ProcessWorker and return the result
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <param name="action"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <param name="p4"></param>
        /// <param name="p5"></param>
        /// <param name="p6"></param>
        /// <param name="p7"></param>
        /// <param name="p8"></param>
        /// <param name="p9"></param>
        /// <param name="maxWait"></param>
        /// <returns></returns>
        public static TResult RunAndWait<TResult, T1, T2, T3, T4, T5, T6, T7, T8, T9>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> action, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, long maxWait = -1)
        {
            using (var worker = Create())
            {
                return worker.ExecuteWait(action, p1, p2, p3, p4, p5, p6, p7, p8, p9, maxWait);
            }
        }

        /// <summary>
        /// Run a workjob with a new ProcessWorker and return the result
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <typeparam name="T10"></typeparam>
        /// <param name="action"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <param name="p4"></param>
        /// <param name="p5"></param>
        /// <param name="p6"></param>
        /// <param name="p7"></param>
        /// <param name="p8"></param>
        /// <param name="p9"></param>
        /// <param name="p10"></param>
        /// <param name="maxWait"></param>
        /// <returns></returns>
        public static TResult RunAndWait<TResult, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> action, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, long maxWait = -1)
        {
            using (var worker = Create())
            {
                return worker.ExecuteWait(action, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, maxWait);
            }
        }

        #endregion RunAndWait with Func

        #region RunAndWait with Action

        /// <summary>
        /// Run a workjob with a new ProcessWorker and wait until its done
        /// </summary>
        /// <param name="action"></param>
        /// <param name="maxWait"></param>
        public static void RunAndWait(Action action, long maxWait = -1)
        {
            using (var worker = Create())
            {
                worker.ExecuteWait(action, maxWait);
            }
        }

        /// <summary>
        /// Run a workjob with a new ProcessWorker and wait until its done
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <param name="action"></param>
        /// <param name="p1"></param>
        /// <param name="maxWait"></param>
        public static void RunAndWait<T1>(Action<T1> action, T1 p1, long maxWait = -1)
        {
            using (var worker = Create())
            {
                worker.ExecuteWait(action, p1, maxWait);
            }
        }

        /// <summary>
        /// Run a workjob with a new ProcessWorker and wait until its done
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="action"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="maxWait"></param>
        public static void RunAndWait<T1, T2>(Action<T1, T2> action, T1 p1, T2 p2, long maxWait = -1)
        {
            using (var worker = Create())
            {
                worker.ExecuteWait(action, p1, p2, maxWait);
            }
        }

        /// <summary>
        /// Run a workjob with a new ProcessWorker and wait until its done
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <param name="action"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <param name="maxWait"></param>
        public static void RunAndWait<T1, T2, T3>(Action<T1, T2, T3> action, T1 p1, T2 p2, T3 p3, long maxWait = -1)
        {
            using (var worker = Create())
            {
                worker.ExecuteWait(action, p1, p2, p3, maxWait);
            }
        }

        /// <summary>
        /// Run a workjob with a new ProcessWorker and wait until its done
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <param name="action"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <param name="p4"></param>
        /// <param name="maxWait"></param>
        public static void RunAndWait<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, T1 p1, T2 p2, T3 p3, T4 p4, long maxWait = -1)
        {
            using (var worker = Create())
            {
                worker.ExecuteWait(action, p1, p2, p3, p4, maxWait);
            }
        }

        /// <summary>
        /// Run a workjob with a new ProcessWorker and wait until its done
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <param name="action"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <param name="p4"></param>
        /// <param name="p5"></param>
        /// <param name="maxWait"></param>
        public static void RunAndWait<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> action, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, long maxWait = -1)
        {
            using (var worker = Create())
            {
                worker.ExecuteWait(action, p1, p2, p3, p4, p5, maxWait);
            }
        }

        /// <summary>
        /// Run a workjob with a new ProcessWorker and wait until its done
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <param name="action"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <param name="p4"></param>
        /// <param name="p5"></param>
        /// <param name="p6"></param>
        /// <param name="maxWait"></param>
        public static void RunAndWait<T1, T2, T3, T4, T5, T6>(Action<T1, T2, T3, T4, T5, T6> action, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, long maxWait = -1)
        {
            using (var worker = Create())
            {
                worker.ExecuteWait(action, p1, p2, p3, p4, p5, p6, maxWait);
            }
        }

        /// <summary>
        /// Run a workjob with a new ProcessWorker and wait until its done
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <param name="action"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <param name="p4"></param>
        /// <param name="p5"></param>
        /// <param name="p6"></param>
        /// <param name="p7"></param>
        /// <param name="maxWait"></param>
        public static void RunAndWait<T1, T2, T3, T4, T5, T6, T7>(Action<T1, T2, T3, T4, T5, T6, T7> action, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, long maxWait = -1)
        {
            using (var worker = Create())
            {
                worker.ExecuteWait(action, p1, p2, p3, p4, p5, p6, p7, maxWait);
            }
        }

        /// <summary>
        /// Run a workjob with a new ProcessWorker and wait until its done
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <param name="action"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <param name="p4"></param>
        /// <param name="p5"></param>
        /// <param name="p6"></param>
        /// <param name="p7"></param>
        /// <param name="p8"></param>
        /// <param name="maxWait"></param>
        public static void RunAndWait<T1, T2, T3, T4, T5, T6, T7, T8>(Action<T1, T2, T3, T4, T5, T6, T7, T8> action, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, long maxWait = -1)
        {
            using (var worker = Create())
            {
                worker.ExecuteWait(action, p1, p2, p3, p4, p5, p6, p7, p8, maxWait);
            }
        }

        /// <summary>
        /// Run a workjob with a new ProcessWorker and wait until its done
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <param name="action"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <param name="p4"></param>
        /// <param name="p5"></param>
        /// <param name="p6"></param>
        /// <param name="p7"></param>
        /// <param name="p8"></param>
        /// <param name="p9"></param>
        /// <param name="maxWait"></param>
        public static void RunAndWait<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> action, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, long maxWait = -1)
        {
            using (var worker = Create())
            {
                worker.ExecuteWait(action, p1, p2, p3, p4, p5, p6, p7, p8, p9, maxWait);
            }
        }

        /// <summary>
        /// Run a workjob with a new ProcessWorker and wait until its done
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <typeparam name="T10"></typeparam>
        /// <param name="action"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <param name="p4"></param>
        /// <param name="p5"></param>
        /// <param name="p6"></param>
        /// <param name="p7"></param>
        /// <param name="p8"></param>
        /// <param name="p9"></param>
        /// <param name="p10"></param>
        /// <param name="maxWait"></param>
        public static void RunAndWait<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> action, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, long maxWait = -1)
        {
            using (var worker = Create())
            {
                worker.ExecuteWait(action, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, maxWait);
            }
        }

        #endregion RunAndWait with Action

    }
}