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

        internal static string MultiProcessWorkerExePath => AssemblyHelper.CurrentAssemblyPath;

        #endregion Properties

        #region Events

        public event EventHandler<ExitCode> RemoteProcessExit;
        public event EventHandler<WorkCompleteEventArgs> WorkComplete;

        #endregion Events

        #region Constructor / Dispose

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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~MultiProcessWorkerClient()
        {
            Dispose(false);
        }

        #endregion Constructor / Dispose

        #region Public

        public Guid Execute<TResult>(Func<TResult> action) where TResult : class
        {
            IsDisposedCheck();

            var newWorkCommand = WorkCommand.Create(action.Method.DeclaringType, action.Method.Name);

            return StartWork(newWorkCommand);
        }

        public Guid Execute<T1, TResult>(Func<T1, TResult> action, T1 p1) where TResult : class
        {
            IsDisposedCheck();

            var newWorkCommand = WorkCommand.Create(action.Method.DeclaringType, action.Method.Name, new object[] { p1 } );

            return StartWork(newWorkCommand);
        }

        public TResult ExecuteWait<TResult>(Func<TResult> action, long maxWait = -1) where TResult : class
        {
            IsDisposedCheck();

            var guid = Execute(action);

            return WaitForWorkDone<TResult>(maxWait, guid);
        }

        public TResult ExecuteWait<TResult, TIn>(Func<TIn, TResult> action, TIn p1, long maxWait = -1) where TResult : class
        {
            IsDisposedCheck();

            var guid = Execute(action, p1);

            return WaitForWorkDone<TResult>(maxWait, guid);
        }

        public bool IsDataReady(Guid guid)
        {
            IsDisposedCheck();

            return m_WorkCommandResults.ContainsKey(guid);
        }

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
                CreateNoWindow = false
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