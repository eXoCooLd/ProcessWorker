using MultiProcessWorker.Private.Helper;
using MultiProcessWorker.Private.Ipc;
using MultiProcessWorker.Private.ProcessData;
using MultiProcessWorker.Public.ProcessData;
using MultiProcessWorker.Public.WorkItems;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace MultiProcessWorker.Private.MultiProcessWorkerLogic
{
    internal sealed class MultiProcessWorkerRunner : IDisposable
    {
        private bool m_Run;

        private readonly ProcessArguments m_ProcessArguments;
        private readonly Queue<WorkCommand> m_WorkCommands;

        private Process m_ParentProcess;

        private MultiProcessWorkerRunner(ProcessArguments processArguments)
        {
            m_ProcessArguments = processArguments;

            AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainUnhandledException;

            m_ParentProcess = Process.GetProcessById(processArguments.IpcParentProgramPid);
            m_ParentProcess.EnableRaisingEvents = true;
            m_ParentProcess.Exited += OnParentProcessExited;

            m_WorkCommands = new Queue<WorkCommand>();
        }
        public void Dispose()
        {
            m_Run = false;

            if (m_ParentProcess != null)
            {
                AppDomain.CurrentDomain.UnhandledException -= OnCurrentDomainUnhandledException;

                m_ParentProcess.Exited -= OnParentProcessExited;
                m_ParentProcess.Dispose();
                m_ParentProcess = null;
            }
        }

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

        private void WorkThreadMain(IpcCommunication<WorkCommand, WorkResult> ipcCommunication)
        {
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
            var methodInfo = workItem.GetMethodInfo();
            if (methodInfo == null)
            {
                return;
            }

            try
            {
                var result = methodInfo.DoWork();
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
    }
}