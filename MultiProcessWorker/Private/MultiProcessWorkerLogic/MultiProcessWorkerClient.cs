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

        public Guid Execute<T>(Func<T> action) where T : class
        {
            IsDisposedCheck();

            var newWorkCommand = WorkCommand.Create(action.Method.DeclaringType, action.Method.Name);

            var methodInfo = newWorkCommand.GetMethodInfo();
            if (!methodInfo.CheckMethodInfo())
            {
                return Guid.Empty;
            }

            m_IpcCommunication.SendData(newWorkCommand);

            return newWorkCommand.Guid;
        }

        public T ExecuteWait<T>(Func<T> action, long maxWait = -1) where T : class
        {
            IsDisposedCheck();

            var guid = Execute(action);

            var timeOut = GetTimeOut(maxWait);

            do
            {
                if (IsDataReady(guid))
                {
                    return GetData<T>(guid);
                }

                if (m_WorkerProcess.HasExited)
                {
                    throw new ProcessWorkerCrashedException(m_WorkerProcess, ExitCode.ErrorCrash);
                }

                Thread.Sleep(1);

            } while (DateTime.UtcNow.Ticks < timeOut);

            return default(T);
        }

        public bool IsDataReady(Guid guid)
        {
            IsDisposedCheck();

            return m_WorkCommandResults.ContainsKey(guid);
        }

        public T GetResult<T>(Guid guid) where T : class
        {
            IsDisposedCheck();

            return IsDataReady(guid) ? GetData<T>(guid) : default(T);
        }

        #endregion Public

        #region Internal

        internal void IsDisposedCheck()
        {
            if (m_IsDisposed)
            {
                throw new ObjectDisposedException(nameof(MultiProcessWorkerClient));
            }
        }

        internal T GetData<T>(Guid guid) where T : class
        {
            if (m_WorkCommandResults.TryRemove(guid, out var workResult))
            {
                if (workResult.Exception == null)
                {
                    return workResult.Result as T;
                }

                throw new ProcessWorkerRemoteException(workResult.Exception);
            }

            return default(T);
        }

        internal void OnIpcCommunicationMessageRecived(object sender, WorkResult e)
        {
            m_WorkCommandResults[e.Guid] = e;
            WorkComplete?.Invoke(this, new WorkCompleteEventArgs(e.Guid));
        }

        internal void OnWorkerProcessDisposed(object sender, EventArgs e)
        {
            HandleProcessExit(sender);
        }

        internal void OnWorkerProcessExited(object sender, EventArgs e)
        {
            HandleProcessExit(sender);
        }

        internal void HandleProcessExit(object sender)
        {
            if (sender is Process process)
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

        internal static long GetTimeOut(long maxWait)
        {
            var timeOut = maxWait > 0 ? DateTime.UtcNow.Ticks + maxWait * TimeSpan.TicksPerMillisecond : long.MaxValue;

            return timeOut;
        }

        internal Process CreateProcess(string processArguments)
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