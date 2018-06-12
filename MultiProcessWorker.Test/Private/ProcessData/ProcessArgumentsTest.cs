using MultiProcessWorker.Private.ProcessData;
using NUnit.Framework;
using System;
using System.Diagnostics;

namespace MultiProcessWorker.Test.Private.ProcessData
{
    [TestFixture]
    public class ProcessArgumentsTest
    {
        [Test]
        public void CreateFromNameTest()
        {
            const string ipcName = "TestMe";
            var processArguments = ProcessArguments.Create(ipcName, typeof(ProcessArgumentsTest));

            Assert.IsTrue(processArguments.IsValid);
            Assert.AreEqual(processArguments.IpcClientName, ProcessArguments.CreateIpcClientName(ipcName));
            Assert.AreEqual(processArguments.IpcProcessName, ProcessArguments.CreateIpcProcessName(ipcName));
            Assert.AreEqual(processArguments.IpcServerName, ProcessArguments.CreateIpcServerName(ipcName));
            Assert.AreEqual(processArguments.IpcParentProgramPid, Process.GetCurrentProcess().Id);
            Assert.AreEqual(typeof(ProcessArgumentsTest).Assembly, processArguments.IpcAssembly);
            Assert.AreEqual(typeof(ProcessArgumentsTest), processArguments.IpcRemoteType);
        }

        [Test]
        public void CreateFromArgsTest()
        {
            const string ipcName = "TestMe";
            var processArguments = ProcessArguments.Create(ipcName, typeof(ProcessArgumentsTest));

            var args = processArguments.ToStringArray();
            var processArgumentsArgs = ProcessArguments.Create(args);

            Assert.IsTrue(processArgumentsArgs.IsValid);
            Assert.AreEqual(processArguments.IpcClientName, processArgumentsArgs.IpcClientName);
            Assert.AreEqual(processArgumentsArgs.IpcProcessName, processArgumentsArgs.IpcProcessName);
            Assert.AreEqual(processArguments.IpcServerName, processArgumentsArgs.IpcServerName);
            Assert.AreEqual(processArguments.IpcParentProgramPid, processArgumentsArgs.IpcParentProgramPid);
            Assert.AreEqual(processArguments.IpcAssembly, processArgumentsArgs.IpcAssembly);
            Assert.AreEqual(processArguments.IpcRemoteType, processArgumentsArgs.IpcRemoteType);
        }

        [Test]
        public void ToStringTest()
        {
            const string ipcName = "TestMe";
            var processArguments = ProcessArguments.Create(ipcName, typeof(ProcessArgumentsTest));

            var argsString = processArguments.ToString();

            var args = argsString.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            var processArgumentsArgs = ProcessArguments.Create(args);

            Assert.IsTrue(processArgumentsArgs.IsValid);
            Assert.AreEqual(processArguments.IpcClientName, processArgumentsArgs.IpcClientName);
            Assert.AreEqual(processArgumentsArgs.IpcProcessName, processArgumentsArgs.IpcProcessName);
            Assert.AreEqual(processArguments.IpcServerName, processArgumentsArgs.IpcServerName);
            Assert.AreEqual(processArguments.IpcParentProgramPid, processArgumentsArgs.IpcParentProgramPid);
            Assert.AreEqual(processArguments.IpcAssembly, processArgumentsArgs.IpcAssembly);
            Assert.AreEqual(processArguments.IpcRemoteType, processArgumentsArgs.IpcRemoteType);
        }

        [Test]
        public void CreateFromArgsFailTest()
        {
            var processArgumentsArgs = ProcessArguments.Create(new []{"blabla"});
            Assert.IsNull(processArgumentsArgs);

            var processArgumentsArgs2 = ProcessArguments.Create(new[] { "blabla=" });
            Assert.IsNull(processArgumentsArgs2);
        }
    }
}
