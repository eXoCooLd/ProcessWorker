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
            var processArguments = ProcessArguments.Create(ipcName);

            Assert.IsTrue(processArguments.IsValid);
            Assert.AreEqual(processArguments.IpcClientName, ProcessArguments.CreateIpcClientName(ipcName));
            Assert.AreEqual(processArguments.IpcProcessName, ProcessArguments.CreateIpcProcessName(ipcName));
            Assert.AreEqual(processArguments.IpcServerName, ProcessArguments.CreateIpcServerName(ipcName));
            Assert.AreEqual(processArguments.IpcParentProgramPid, Process.GetCurrentProcess().Id);
        }

        [Test]
        public void CreateFromArgsTest()
        {
            const string ipcName = "TestMe";
            var processArguments = ProcessArguments.Create(ipcName);

            var args = processArguments.ToStringArray();
            var processArgumentsArgs = ProcessArguments.Create(args);

            Assert.IsTrue(processArgumentsArgs.IsValid);
            Assert.AreEqual(processArguments.IpcClientName, processArgumentsArgs.IpcClientName);
            Assert.AreEqual(processArgumentsArgs.IpcProcessName, processArgumentsArgs.IpcProcessName);
            Assert.AreEqual(processArguments.IpcServerName, processArgumentsArgs.IpcServerName);
            Assert.AreEqual(processArguments.IpcParentProgramPid, processArgumentsArgs.IpcParentProgramPid);
        }

        [Test]
        public void ToStringTest()
        {
            const string ipcName = "TestMe";
            var processArguments = ProcessArguments.Create(ipcName);

            var argsString = processArguments.ToString();

            var args = argsString.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            var processArgumentsArgs = ProcessArguments.Create(args);

            Assert.IsTrue(processArgumentsArgs.IsValid);
            Assert.AreEqual(processArguments.IpcClientName, processArgumentsArgs.IpcClientName);
            Assert.AreEqual(processArgumentsArgs.IpcProcessName, processArgumentsArgs.IpcProcessName);
            Assert.AreEqual(processArguments.IpcServerName, processArgumentsArgs.IpcServerName);
            Assert.AreEqual(processArguments.IpcParentProgramPid, processArgumentsArgs.IpcParentProgramPid);
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
