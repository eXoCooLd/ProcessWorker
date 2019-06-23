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
using MultiProcessWorker.Public.ProcessData;
using MultiProcessWorker.Public.WorkItems;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;
#endregion Used Namespaces

namespace MultiProcessWorker.Private.MultiProcessWorkerLogic
{
    /// <summary>
    /// Base Multi Process Worker on Clientside
    /// </summary>
    internal abstract class MultiProcessWorkerClientBase : IDisposable
    {

        #region Fields

        /// <summary>
        /// Dictionary with recived work results
        /// </summary>
        private readonly ConcurrentDictionary<Guid, WorkResult> m_WorkCommandResults;

        /// <summary>
        /// Interprocess communication to the remote process
        /// </summary>
        private IpcCommunication<WorkCommand, WorkResult> m_IpcCommunication;

        /// <summary>
        /// Remote Process Wait Handle
        /// </summary>
        private EventWaitHandle m_ProcessEventWaitHandle;

        /// <summary>
        /// Remote Process
        /// </summary>
        private Process m_WorkerProcess;

        /// <summary>
        /// Dispose Status
        /// </summary>
        private bool m_IsDisposed;

        #endregion Fields

        #region Properties

        /// <summary>
        /// Path to the ProcessWorker.exe
        /// </summary>
        internal static string MultiProcessWorkerExePath => AssemblyHelper.CurrentAssemblyPath;

        /// <summary>
        /// Type of the HostedObject
        /// </summary>
        public Type HostedObjecType { get; }

        /// <summary>
        /// Maximum Timeout for the shutdown of the Processworker with a hosted Object
        /// </summary>
        public int MaxShutdownTimeout { get; }

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
        protected MultiProcessWorkerClientBase(string ipcName) : this(ProcessArguments.Create(ipcName))
        {
            MaxShutdownTimeout = 0;
        }

        /// <summary>
        /// Create a remote worker with a hosted object
        /// </summary>
        /// <param name="ipcName"></param>
        /// <param name="maxShutdownTimeout"></param>
        /// <param name="remoteType"></param>
        protected MultiProcessWorkerClientBase(string ipcName, int maxShutdownTimeout, Type remoteType) : this(ProcessArguments.Create(ipcName, remoteType))
        {
            MaxShutdownTimeout = maxShutdownTimeout;
        }

        /// <summary>
        /// Create a remote worker
        /// </summary>
        /// <param name="processArguments"></param>
        private MultiProcessWorkerClientBase(ProcessArguments processArguments)
        {
            HostedObjecType = processArguments.IpcRemoteType;

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
                WaitForRemoteProcessShutdown();

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
        ~MultiProcessWorkerClientBase()
        {
            Dispose(false);
        }

        #endregion Constructor / Dispose

        #region Public

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
        public TResult GetResult<TResult>(Guid guid)
        {
            IsDisposedCheck();

            return IsDataReady(guid) ? GetData<TResult>(guid) : default(TResult);
        }

        #endregion Public

        #region Protected

        /// <summary>
        /// Check the Dispose State
        /// </summary>
        protected void IsDisposedCheck()
        {
            if (m_IsDisposed)
            {
                throw new ObjectDisposedException(nameof(MultiProcessWorkerClient));
            }
        }

        /// <summary>
        /// Build a new WorkCommand
        /// </summary>
        /// <param name="action"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        protected WorkCommand BuildWorkCommand(Delegate action, object[] parameters = null)
        {
            var objectType = HostedObjecType ?? action.Method.DeclaringType;
            var parameterTypes = action.Method.GetParameters().Select(p => p.ParameterType).ToArray();
            var newWorkCommand = WorkCommand.Create(objectType, action.Method.Name, parameters, parameterTypes);

            return newWorkCommand;
        }

        /// <summary>
        /// Start a new work job
        /// </summary>
        /// <param name="newWorkCommand"></param>
        /// <returns></returns>
        protected Guid StartWork(WorkCommand newWorkCommand)
        {
            var methodInfo = newWorkCommand.GetMethodInfo(HostedObjecType);
            if (!methodInfo.CheckMethodInfo())
            {
                return Guid.Empty;
            }

            m_IpcCommunication.SendData(newWorkCommand);

            return newWorkCommand.Guid;
        }

        /// <summary>
        /// Wait until the work job is done or the timeout kicks in
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="maxWait"></param>
        /// <param name="guid"></param>
        /// <returns>Work result if the job is done</returns>
        protected TResult WaitForWorkDone<TResult>(long maxWait, Guid guid)
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

        /// <summary>
        /// Wait until the work job is done or the timeout kicks in
        /// </summary>
        /// <param name="maxWait"></param>
        /// <param name="guid"></param>
        /// <returns>true if the job is done</returns>
        protected bool WaitForWorkDone(long maxWait, Guid guid)
        {
            var timeOut = GetTimeOut(maxWait);

            do
            {
                if (IsDataReady(guid))
                {
                    return CheckData(guid);
                }

                if (m_WorkerProcess.HasExited)
                {
                    throw new ProcessWorkerCrashedException(m_WorkerProcess, ExitCode.ErrorCrash);
                }

                Thread.Sleep(1);
            } while (DateTime.UtcNow.Ticks < timeOut);

            return false;
        }

        #endregion Protected

        #region Private

        /// <summary>
        /// Get the result data from a work job without a return value
        /// </summary>
        /// <param name="guid"></param>
        private bool CheckData(Guid guid)
        {
            WorkResult workResult;
            if (m_WorkCommandResults.TryRemove(guid, out workResult))
            {
                if (workResult.Exception == null)
                {
                    return true;
                }

                throw new ProcessWorkerRemoteException(workResult.Exception);
            }

            return false;
        }

        /// <summary>
        /// Get the result data from a work job
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="guid"></param>
        /// <returns></returns>
        private TResult GetData<TResult>(Guid guid)
        {
            WorkResult workResult;
            if (m_WorkCommandResults.TryRemove(guid, out workResult))
            {
                if (workResult.Exception == null)
                {
                    return ConvertDataToType<TResult>(workResult.Result);
                }

                throw new ProcessWorkerRemoteException(workResult.Exception);
            }

            return default(TResult);
        }

        /// <summary>
        /// Convert the workjob data back to the requested type
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        private static TResult ConvertDataToType<TResult>(object data)
        {
            if (data == null)
            {
                return default(TResult);
            }

            TResult result;

            try
            {
                var tResultType = typeof(TResult);
                if (tResultType.IsValueType)
                {
                    if (tResultType.IsEnum)
                    {
                        result = (TResult)Enum.Parse(typeof(TResult), data.ToString());
                    }
                    else
                    {
                        result = (TResult)Convert.ChangeType(data, typeof(TResult));
                    }
                }
                else
                {
                    result = (TResult)data;
                }
            }
            catch (Exception)
            {
                result = default(TResult);
            }

            return result;
        }

        /// <summary>
        /// Interprocess messege recived handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnIpcCommunicationMessageRecived(object sender, WorkResult e)
        {
            m_WorkCommandResults[e.Guid] = e;
            WorkComplete?.Invoke(this, new WorkCompleteEventArgs(e.Guid));
        }

        private void OnWorkerProcessDisposed(object sender, EventArgs e)
        {
            HandleProcessExit(sender);
        }

        /// <summary>
        /// Remote process exit event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWorkerProcessExited(object sender, EventArgs e)
        {
            HandleProcessExit(sender);
        }

        /// <summary>
        /// Handle the remote process exit event
        /// </summary>
        /// <param name="sender"></param>
        private void HandleProcessExit(object sender)
        {
            var process = sender as Process;
            if (process != null)
            {
                var exitCode = GetExitCodeFromProcess(process);
                RemoteProcessExit?.Invoke(this, exitCode);

                if (exitCode != ExitCode.Ok)
                {
                    Dispose();
                    throw new ProcessWorkerCrashedException(process, exitCode);
                }
            }
        }

        /// <summary>
        /// Get the ExitCode from the Process
        /// </summary>
        /// <param name="process"></param>
        /// <returns></returns>
        private static ExitCode GetExitCodeFromProcess(Process process)
        {
            ExitCode exitCode;

            try
            {
                exitCode = (ExitCode)process.ExitCode;
            }
            catch (InvalidOperationException)
            {
                return ExitCode.ErrorCrash;
            }

            return exitCode;
        }

        /// <summary>
        /// Get the current timeout value
        /// </summary>
        /// <param name="maxWait"></param>
        /// <returns></returns>
        private static long GetTimeOut(long maxWait)
        {
            var timeOut = maxWait > 0 ? DateTime.UtcNow.Ticks + maxWait * TimeSpan.TicksPerMillisecond : long.MaxValue;

            return timeOut;
        }

        /// <summary>
        /// Create the remote worker process
        /// </summary>
        /// <param name="processArguments"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Wait until the remote process is stoped or the timeout is raised
        /// </summary>
        private void WaitForRemoteProcessShutdown()
        {
            if (MaxShutdownTimeout > 0)
            {
                var timeOut = GetTimeOut(MaxShutdownTimeout);

                do
                {
                    Thread.Sleep(1);
                    if (m_WorkerProcess.HasExited)
                    {
                        break;
                    }
                } while (DateTime.UtcNow.Ticks < timeOut);
            }
        }

        #endregion Internal

    }
}
