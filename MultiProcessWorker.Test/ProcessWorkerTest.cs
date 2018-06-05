using MultiProcessWorker.Public.Exceptions;
using NUnit.Framework;
using System;

namespace MultiProcessWorker.Test
{
    [TestFixture]
    public class ProcessWorkerTest
    {
        private const string TestData = "Test123";

        [Test]
        public void ProcessWorkerOkTest()
        {
            var data = ProcessWorker.RunAndWait(RemoteExecute);
            Assert.AreEqual(TestData, data);
        }

        [Test]
        public void ProcessWorkerFailTest()
        {
            Assert.Throws<ArgumentException>(() => ProcessWorker.RunAndWait(RemoteFailExecute));
        }

        [Test]
        public void ProcessWorkerRemoteExceptionTest()
        {
            Assert.Throws<ProcessWorkerRemoteException>(() => ProcessWorker.RunAndWait(RemoteExceptionExecute));
        }

        [Test]
        public void ProcessWorkerWithParameterOkTest()
        {
            const string input = " input test";
            var data = ProcessWorker.RunAndWait(RemoteExeute, input);
            Assert.AreEqual(TestData + input, data);
        }

        public static string RemoteExecute()
        {
            return TestData;
        }

        public string RemoteFailExecute()
        {
            return "nope not static";
        }

        public static string RemoteExceptionExecute()
        {
            throw new InvalidOperationException("Remote Exception!");
        }

        public static string RemoteExeute(string input)
        {
            return TestData + input;
        }
    }
}
