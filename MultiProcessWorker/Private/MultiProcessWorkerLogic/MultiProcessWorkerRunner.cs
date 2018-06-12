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
using MultiProcessWorker.Public.ProcessData;
using MultiProcessWorker.Public.WorkItems;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
#endregion Used Namespaces

namespace MultiProcessWorker.Private.MultiProcessWorkerLogic
{
    /// <summary>
    /// ProcessWorker runner
    /// </summary>
    internal sealed class MultiProcessWorkerRunner : IDisposable
    {

        #region Fields

        private bool m_Run;

        private readonly ProcessArguments m_ProcessArguments;
        private readonly Queue<WorkCommand> m_WorkCommands;

        private Process m_ParentProcess;

        private object m_HostedObject;
        private readonly Type m_HostedType;

        #endregion Fields

        #region Properties

        internal object HostedObject
        {
            get
            {
                if (m_HostedObject == null && m_ProcessArguments.IpcRemoteType != null)
                {
                    m_HostedObject = m_ProcessArguments.IpcRemoteType.CreateInstance();
                }

                return m_HostedObject;
            }
        }

        #endregion Properties

        #region Constructor / Dispose

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="processArguments"></param>
        internal MultiProcessWorkerRunner(ProcessArguments processArguments)
        {
            m_ProcessArguments = processArguments;
            m_HostedType = processArguments.IpcRemoteType;

            AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainUnhandledException;

            m_ParentProcess = Process.GetProcessById(processArguments.IpcParentProgramPid);
            m_ParentProcess.EnableRaisingEvents = true;
            m_ParentProcess.Exited += OnParentProcessExited;

            m_WorkCommands = new Queue<WorkCommand>();
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            m_Run = false;

            if (m_HostedObject != null)
            {
                (m_HostedObject as IDisposable)?.Dispose();
                m_HostedObject = null;
            }

            if (m_ParentProcess != null)
            {
                AppDomain.CurrentDomain.UnhandledException -= OnCurrentDomainUnhandledException;

                m_ParentProcess.Exited -= OnParentProcessExited;
                m_ParentProcess.Dispose();
                m_ParentProcess = null;
            }
        }

        #endregion Constructor / Dispose

        #region Public

        public static MultiProcessWorkerRunner Create(string[] args)
        {
            var processArguments = ProcessArguments.Create(args);
            return processArguments != null ? new MultiProcessWorkerRunner(processArguments) : null;
        }

        public void Run()
        {
            m_Run = !m_ParentProcess.HasExited;
            if (!m_Run)
            {
                Environment.ExitCode = (int)ExitCode.ErrorParentCrash;
                return;
            }

            var exitThread = new Thread(ExitProcessThreadMain);
            exitThread.Start();

            using (var ipcCommunication = IpcCommunication<WorkCommand, WorkResult>.CreateClient(m_ProcessArguments.IpcServerName, m_ProcessArguments.IpcClientName))
            {
                ipcCommunication.MessageRecived += OnCommunicationMessageRecived;

                WorkThreadMain(ipcCommunication);

                ipcCommunication.MessageRecived -= OnCommunicationMessageRecived;
            }

            exitThread.Join();

            Environment.Exit((int)ExitCode.Ok);
        }

        #endregion Public

        #region Private

        private void WorkThreadMain(IpcCommunication<WorkCommand, WorkResult> ipcCommunication)
        {
            if (m_HostedType != null)
            {
                if (HostedObject == null)
                {
                    throw new NullReferenceException(nameof(HostedObject));
                }
            }

            while (m_Run)
            {
                if (m_WorkCommands.Count > 0)
                {
                    DoWork(ipcCommunication);
                }

                Thread.Sleep(1);
            }
        }

        private void OnCommunicationMessageRecived(object sender, WorkResult e)
        {
            m_WorkCommands.Enqueue(e);
        }

        private void DoWork(IpcCommunication<WorkCommand, WorkResult> ipcCommunication)
        {
            var workItem = m_WorkCommands.Dequeue();
            var methodInfo = workItem.GetMethodInfo(m_HostedType);
            if (methodInfo == null)
            {
                ipcCommunication.SendData(WorkResult.Create(workItem, new InvalidOperationException("methodInfo empty!")));
                return;
            }

            try
            {
                var result = methodInfo.Execute(HostedObject, workItem.Parameter);
                ipcCommunication.SendData(WorkResult.Create(workItem, result));
            }
            catch (Exception exception)
            {
                ipcCommunication.SendData(WorkResult.Create(workItem, exception));
            }
        }

        private void ExitProcessThreadMain()
        {
            using (var eventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, m_ProcessArguments.IpcProcessName))
            {
                while (m_Run)
                {
                    if (eventWaitHandle.WaitOne(0))
                    {
                        m_Run = false;
                    }
                    Thread.Sleep(1);
                }
            }
        }

        private void OnCurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e);
            Dispose();
            Environment.Exit((int)ExitCode.ErrorCrash);
        }

        private void OnParentProcessExited(object sender, EventArgs e)
        {
            Dispose();
            Environment.Exit((int)ExitCode.ErrorParentCrash);
        }

        #endregion Private
    }
}