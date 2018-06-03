using System;
using MultiProcessWorker.Private.Ipc;
using NUnit.Framework;
using System.Collections.Generic;
using System.Diagnostics;

namespace MultiProcessWorker.Test.Private.Ipc
{
    [TestFixture]
    public class IpcTest
    {
        private const string TestEntry = "Test Data";
        private Dictionary<string, bool> m_TestData;

        public Dictionary<string, bool> TestData
        {
            get
            {
                if (m_TestData == null)
                {
                    m_TestData = new Dictionary<string, bool>();
                    for (int i = 0; i < 1337; i++)
                    {
                        m_TestData.Add(TestEntry + i, false);
                    }
                }

                return m_TestData;
            }
        }

        [Test]
        public void TestIpc()
        {
            const string serverName = "server";
            const string clientName = "client";

            Console.WriteLine($"Starting ipc for {TestData.Count} dataitems");
            var stopWatch = Stopwatch.StartNew();

            using (var ipcCommunicationClient = IpcCommunication<string, string>.CreateClient(serverName, clientName))
            {
                ipcCommunicationClient.MessageRecived += OnIpcCommunicationClientMessageRecived;

                using (var ipcCommunicationServer = IpcCommunication<string, string>.CreateServer(serverName, clientName))
                {
                    foreach (var key in TestData.Keys)
                    {
                        ipcCommunicationServer.SendData(key);
                    }

                    ipcCommunicationServer.WaitForAllSends();
                }

                ipcCommunicationClient.MessageRecived -= OnIpcCommunicationClientMessageRecived;
            }

            stopWatch.Stop();
            Console.WriteLine($"End ipc Test for {TestData.Count} dataitems in {stopWatch.ElapsedMilliseconds} ms");

            foreach (var value in TestData.Values)
            {
                Assert.IsTrue(value, "Missing Data!");
            }
        }

        private void OnIpcCommunicationClientMessageRecived(object sender, string e)
        {
            Assert.IsTrue(TestData.ContainsKey(e), "Invalid Data!");
            TestData[e] = true;
        }
    }
}
