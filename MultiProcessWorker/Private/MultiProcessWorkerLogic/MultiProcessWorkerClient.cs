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
using MultiProcessWorker.Private.Helper;
using MultiProcessWorker.Private.Ipc;
using MultiProcessWorker.Private.ProcessData;
using MultiProcessWorker.Public.EventArgs;
using MultiProcessWorker.Public.Exceptions;
using MultiProcessWorker.Public.Interfaces;
using MultiProcessWorker.Public.ProcessData;
using MultiProcessWorker.Public.WorkItems;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
#endregion Used Namespaces

namespace MultiProcessWorker.Private.MultiProcessWorkerLogic
{
    internal class MultiProcessWorkerClient : IMultiProcessWorker
    {

        #region Fields

        private readonly ConcurrentDictionary<Guid, WorkResult> m_WorkCommandResults;

        private IpcCommunication<WorkCommand, WorkResult> m_IpcCommunication;
        private EventWaitHandle m_ProcessEventWaitHandle;

        private Process m_WorkerProcess;

        private bool m_IsDisposed;

        #endregion Fields

        #region Properties

        /// <summary>
        /// Path to the ProcessWorker.exe
        /// </summary>
        internal static string MultiProcessWorkerExePath => AssemblyHelper.CurrentAssemblyPath;

        #endregion Properties

        #region Events

        /// <summary>
        /// Remote Process Exit Event
        /// </summary>
        public event EventHandler<ExitCode> RemoteProcessExit;

        /// <summary>
        /// Work Complete Event
        /// </summary>
        public event EventHandler<WorkCompleteEventArgs> WorkComplete;

        #endregion Events

        #region Constructor / Dispose

        /// <summary>
        /// Constructor with interprocess communication name
        /// </summary>
        /// <param name="ipcName"></param>
        internal MultiProcessWorkerClient(string ipcName)
        {
            var processArguments = ProcessArguments.Create(ipcName);

            m_WorkCommandResults = new ConcurrentDictionary<Guid, WorkResult>();

            m_IpcCommunication = IpcCommunication<WorkCommand, WorkResult>.CreateServer(processArguments.IpcServerName, processArguments.IpcClientName);
            m_IpcCommunication.MessageRecived += OnIpcCommunicationMessageRecived;

            m_ProcessEventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, processArguments.IpcProcessName);

            m_WorkerProcess = CreateProcess(processArguments.ToString());
            m_WorkerProcess.Exited += OnWorkerProcessExited;
            m_WorkerProcess.Disposed += OnWorkerProcessDisposed;
            m_WorkerProcess.Start();
        }

        /// <summary>
        /// Dispose
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            m_IsDisposed = true;

            if (m_ProcessEventWaitHandle != null)
            {
                m_ProcessEventWaitHandle.Set();
                m_ProcessEventWaitHandle.Dispose();
                m_ProcessEventWaitHandle = null;
            }

            if (m_IpcCommunication != null)
            {
                m_IpcCommunication.MessageRecived -= OnIpcCommunicationMessageRecived;
                m_IpcCommunication.Dispose();
                m_IpcCommunication = null;
            }

            if (m_WorkerProcess != null)
            {
                if (!m_WorkerProcess.HasExited)
                {
                    m_WorkerProcess.Kill();
                }

                m_WorkerProcess.Disposed -= OnWorkerProcessDisposed;
                m_WorkerProcess.Exited -= OnWorkerProcessExited;

                m_WorkerProcess.Dispose();
                m_WorkerProcess = null;
            }
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Finalizer
        /// </summary>
        ~MultiProcessWorkerClient()
        {
            Dispose(false);
        }

        #endregion Constructor / Dispose

        #region Public

        /// <summary>
        /// Execute a work job and return it's jobid
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="action"></param>
        /// <returns></returns>
        public Guid Execute<TResult>(Func<TResult> action) where TResult : class
        {
            IsDisposedCheck();

            var newWorkCommand = WorkCommand.Create(action.Method.DeclaringType, action.Method.Name);

            return StartWork(newWorkCommand);
        }

        /// <summary>
        /// Execute a work job and return it's jobid
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="action"></param>
        /// <param name="p1"></param>
        /// <returns></returns>
        public Guid Execute<T1, TResult>(Func<T1, TResult> action, T1 p1) where TResult : class
        {
            IsDisposedCheck();

            var newWorkCommand = WorkCommand.Create(action.Method.DeclaringType, action.Method.Name, new object[] { p1 } );

            return StartWork(newWorkCommand);
        }

        /// <summary>
        /// Execute a work job and return it's jobid
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="action"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public Guid Execute<T1, T2, TResult>(Func<T1, T2, TResult> action, T1 p1, T2 p2) where TResult : class
        {
            IsDisposedCheck();

            var newWorkCommand = WorkCommand.Create(action.Method.DeclaringType, action.Method.Name, new object[] { p1, p2 });

            return StartWork(newWorkCommand);
        }

        /// <summary>
        /// Execute a work job and return it's jobid
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="action"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <returns></returns>
        public Guid Execute<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> action, T1 p1, T2 p2, T3 p3) where TResult : class
        {
            IsDisposedCheck();

            var newWorkCommand = WorkCommand.Create(action.Method.DeclaringType, action.Method.Name, new object[] { p1, p2, p3 });

            return StartWork(newWorkCommand);
        }

        /// <summary>
        /// Execute a work job and return it's jobid
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="action"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <param name="p4"></param>
        /// <returns></returns>
        public Guid Execute<T1, T2, T3, T4, TResult>(Func<T1, T2, T3, T4, TResult> action, T1 p1, T2 p2, T3 p3, T4 p4) where TResult : class
        {
            IsDisposedCheck();

            var newWorkCommand = WorkCommand.Create(action.Method.DeclaringType, action.Method.Name, new object[] { p1, p2, p3, p4 });

            return StartWork(newWorkCommand);
        }

        /// <summary>
        /// Execute a work job and return it's jobid
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="action"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <param name="p4"></param>
        /// <param name="p5"></param>
        /// <returns></returns>
        public Guid Execute<T1, T2, T3, T4, T5, TResult>(Func<T1, T2, T3, T4, T5, TResult> action, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5) where TResult : class
        {
            IsDisposedCheck();

            var newWorkCommand = WorkCommand.Create(action.Method.DeclaringType, action.Method.Name, new object[] { p1, p2, p3, p4, p5 });

            return StartWork(newWorkCommand);
        }

        /// <summary>
        /// Execute a work job and return it's jobid
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="action"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <param name="p4"></param>
        /// <param name="p5"></param>
        /// <param name="p6"></param>
        /// <returns></returns>
        public Guid Execute<T1, T2, T3, T4, T5, T6, TResult>(Func<T1, T2, T3, T4, T5, T6, TResult> action, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6) where TResult : class
        {
            IsDisposedCheck();

            var newWorkCommand = WorkCommand.Create(action.Method.DeclaringType, action.Method.Name, new object[] { p1, p2, p3, p4, p5, p6 });

            return StartWork(newWorkCommand);
        }

        /// <summary>
        /// Execute a work job and return it's jobid
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="action"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <param name="p4"></param>
        /// <param name="p5"></param>
        /// <param name="p6"></param>
        /// <param name="p7"></param>
        /// <returns></returns>
        public Guid Execute<T1, T2, T3, T4, T5, T6, T7, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, TResult> action, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7) where TResult : class
        {
            IsDisposedCheck();

            var newWorkCommand = WorkCommand.Create(action.Method.DeclaringType, action.Method.Name, new object[] { p1, p2, p3, p4, p5, p6, p7 });

            return StartWork(newWorkCommand);
        }

        /// <summary>
        /// Execute a work job and return it's jobid
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="action"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <param name="p4"></param>
        /// <param name="p5"></param>
        /// <param name="p6"></param>
        /// <param name="p7"></param>
        /// <param name="p8"></param>
        /// <returns></returns>
        public Guid Execute<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> action, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8) where TResult : class
        {
            IsDisposedCheck();

            var newWorkCommand = WorkCommand.Create(action.Method.DeclaringType, action.Method.Name, new object[] { p1, p2, p3, p4, p5, p6, p7, p8 });

            return StartWork(newWorkCommand);
        }

        /// <summary>
        /// Execute a work job and return it's jobid
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
        /// <typeparam name="TResult"></typeparam>
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
        /// <returns></returns>
        public Guid Execute<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> action, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9) where TResult : class
        {
            IsDisposedCheck();

            var newWorkCommand = WorkCommand.Create(action.Method.DeclaringType, action.Method.Name, new object[] { p1, p2, p3, p4, p5, p6, p7, p8, p9 });

            return StartWork(newWorkCommand);
        }

        /// <summary>
        /// Execute a work job and return it's jobid
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
        /// <typeparam name="TResult"></typeparam>
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
        /// <returns></returns>
        public Guid Execute<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> action, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10) where TResult : class
        {
            IsDisposedCheck();

            var newWorkCommand = WorkCommand.Create(action.Method.DeclaringType, action.Method.Name, new object[] { p1, p2, p3, p4, p5, p6, p7, p8, p9, p10 });

            return StartWork(newWorkCommand);
        }

        /// <summary>
        /// Execute a work job and wait for the result
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="action"></param>
        /// <param name="maxWait"></param>
        /// <returns></returns>
        public TResult ExecuteWait<TResult>(Func<TResult> action, long maxWait = -1) where TResult : class
        {
            IsDisposedCheck();

            var guid = Execute(action);

            return WaitForWorkDone<TResult>(maxWait, guid);
        }

        /// <summary>
        /// Execute a work job and wait for the result
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <param name="action"></param>
        /// <param name="p1"></param>
        /// <param name="maxWait"></param>
        /// <returns></returns>
        public TResult ExecuteWait<TResult, T1>(Func<T1, TResult> action, T1 p1, long maxWait = -1) where TResult : class
        {
            IsDisposedCheck();

            var guid = Execute(action, p1);

            return WaitForWorkDone<TResult>(maxWait, guid);
        }

        /// <summary>
        /// Execute a work job and wait for the result
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="action"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="maxWait"></param>
        /// <returns></returns>
        public TResult ExecuteWait<TResult, T1, T2>(Func<T1, T2, TResult> action, T1 p1, T2 p2, long maxWait = -1) where TResult : class
        {
            IsDisposedCheck();

            var guid = Execute(action, p1, p2);

            return WaitForWorkDone<TResult>(maxWait, guid);
        }

        /// <summary>
        /// Execute a work job and wait for the result
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
        public TResult ExecuteWait<TResult, T1, T2, T3>(Func<T1, T2, T3, TResult> action, T1 p1, T2 p2, T3 p3, long maxWait = -1) where TResult : class
        {
            IsDisposedCheck();

            var guid = Execute(action, p1, p2, p3);

            return WaitForWorkDone<TResult>(maxWait, guid);
        }

        /// <summary>
        /// Execute a work job and wait for the result
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
        public TResult ExecuteWait<TResult, T1, T2, T3, T4>(Func<T1, T2, T3, T4, TResult> action, T1 p1, T2 p2, T3 p3, T4 p4, long maxWait = -1) where TResult : class
        {
            IsDisposedCheck();

            var guid = Execute(action, p1, p2, p3, p4);

            return WaitForWorkDone<TResult>(maxWait, guid);
        }

        /// <summary>
        /// Execute a work job and wait for the result
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
        public TResult ExecuteWait<TResult, T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5, TResult> action, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, long maxWait = -1) where TResult : class
        {
            IsDisposedCheck();

            var guid = Execute(action, p1, p2, p3, p4, p5);

            return WaitForWorkDone<TResult>(maxWait, guid);
        }

        /// <summary>
        /// Execute a work job and wait for the result
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
        public TResult ExecuteWait<TResult, T1, T2, T3, T4, T5, T6>(Func<T1, T2, T3, T4, T5, T6, TResult> action, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, long maxWait = -1) where TResult : class
        {
            IsDisposedCheck();

            var guid = Execute(action, p1, p2, p3, p4, p5, p6);

            return WaitForWorkDone<TResult>(maxWait, guid);
        }

        /// <summary>
        /// Execute a work job and wait for the result
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
        public TResult ExecuteWait<TResult, T1, T2, T3, T4, T5, T6, T7>(Func<T1, T2, T3, T4, T5, T6, T7, TResult> action, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, long maxWait = -1) where TResult : class
        {
            IsDisposedCheck();

            var guid = Execute(action, p1, p2, p3, p4, p5, p6, p7);

            return WaitForWorkDone<TResult>(maxWait, guid);
        }

        /// <summary>
        /// Execute a work job and wait for the result
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
        public TResult ExecuteWait<TResult, T1, T2, T3, T4, T5, T6, T7, T8>(Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> action, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, long maxWait = -1) where TResult : class
        {
            IsDisposedCheck();

            var guid = Execute(action, p1, p2, p3, p4, p5, p6, p7, p8);

            return WaitForWorkDone<TResult>(maxWait, guid);
        }

        /// <summary>
        /// Execute a work job and wait for the result
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
        public TResult ExecuteWait<TResult, T1, T2, T3, T4, T5, T6, T7, T8, T9>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> action, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, long maxWait = -1) where TResult : class
        {
            IsDisposedCheck();

            var guid = Execute(action, p1, p2, p3, p4, p5, p6, p7, p8, p9);

            return WaitForWorkDone<TResult>(maxWait, guid);
        }

        /// <summary>
        /// Execute a work job and wait for the result
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
        public TResult ExecuteWait<TResult, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> action, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, long maxWait = -1) where TResult : class
        {
            IsDisposedCheck();

            var guid = Execute(action, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10);

            return WaitForWorkDone<TResult>(maxWait, guid);
        }

        /// <summary>
        /// Check if the result of a job is ready
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public bool IsDataReady(Guid guid)
        {
            IsDisposedCheck();

            return m_WorkCommandResults.ContainsKey(guid);
        }

        /// <summary>
        /// Get the result of a job
        /// </summary>
        /// <typeparam name="TResult">Job result</typeparam>
        /// <param name="guid">work job id</param>
        /// <returns></returns>
        public TResult GetResult<TResult>(Guid guid) where TResult : class
        {
            IsDisposedCheck();

            return IsDataReady(guid) ? GetData<TResult>(guid) : default(TResult);
        }

        #endregion Public

        #region Internal

        private void IsDisposedCheck()
        {
            if (m_IsDisposed)
            {
                throw new ObjectDisposedException(nameof(MultiProcessWorkerClient));
            }
        }

        private Guid StartWork(WorkCommand newWorkCommand)
        {
            var methodInfo = newWorkCommand.GetMethodInfo();
            if (!methodInfo.CheckMethodInfo())
            {
                return Guid.Empty;
            }

            m_IpcCommunication.SendData(newWorkCommand);

            return newWorkCommand.Guid;
        }

        private TResult WaitForWorkDone<TResult>(long maxWait, Guid guid) where TResult : class
        {
            var timeOut = GetTimeOut(maxWait);

            do
            {
                if (IsDataReady(guid))
                {
                    return GetData<TResult>(guid);
                }

                if (m_WorkerProcess.HasExited)
                {
                    throw new ProcessWorkerCrashedException(m_WorkerProcess, ExitCode.ErrorCrash);
                }

                Thread.Sleep(1);
            } while (DateTime.UtcNow.Ticks < timeOut);

            return default(TResult);
        }

        private TResult GetData<TResult>(Guid guid) where TResult : class
        {
            WorkResult workResult;
            if (m_WorkCommandResults.TryRemove(guid, out workResult))
            {
                if (workResult.Exception == null)
                {
                    return workResult.Result as TResult;
                }

                throw new ProcessWorkerRemoteException(workResult.Exception);
            }

            return default(TResult);
        }

        private void OnIpcCommunicationMessageRecived(object sender, WorkResult e)
        {
            m_WorkCommandResults[e.Guid] = e;
            WorkComplete?.Invoke(this, new WorkCompleteEventArgs(e.Guid));
        }

        private void OnWorkerProcessDisposed(object sender, EventArgs e)
        {
            HandleProcessExit(sender);
        }

        private void OnWorkerProcessExited(object sender, EventArgs e)
        {
            HandleProcessExit(sender);
        }

        private void HandleProcessExit(object sender)
        {
            var process = sender as Process;
            if (process != null)
            {
                var exitCode = (ExitCode)process.ExitCode;
                RemoteProcessExit?.Invoke(this, exitCode);

                if (exitCode != ExitCode.Ok)
                {
                    Dispose();
                    throw new ProcessWorkerCrashedException(process, exitCode);
                }
            }
        }

        private static long GetTimeOut(long maxWait)
        {
            var timeOut = maxWait > 0 ? DateTime.UtcNow.Ticks + maxWait * TimeSpan.TicksPerMillisecond : long.MaxValue;

            return timeOut;
        }

        private Process CreateProcess(string processArguments)
        {
            var processStartInfo = new ProcessStartInfo(MultiProcessWorkerExePath, processArguments)
            {
                UseShellExecute = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            var workerProcess = new Process
            {
                StartInfo = processStartInfo,
                EnableRaisingEvents = true
            };

            return workerProcess;
        }

        #endregion Internal

    }
}