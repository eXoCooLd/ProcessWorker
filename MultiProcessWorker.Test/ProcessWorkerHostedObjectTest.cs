using MultiProcessWorker.Public.Interfaces;
using NUnit.Framework;
using System;
using System.IO;

namespace MultiProcessWorker.Test
{
    public interface IRemoteClass : IDisposable
    {
        void Set(Int64 value);
        Int64 Get();
        void SetDisposeText(string file, string text);
    }

    public sealed class RemoteClass : IRemoteClass
    {
        private Int64 m_InitialValue;
        private string m_File;
        private string m_DisposeText;

        public RemoteClass()
        {
            m_InitialValue = 1000;
            Console.WriteLine("Initial: " + m_InitialValue);
        }

        public void Set(Int64 value)
        {
            Console.WriteLine("Set: " + value);
            m_InitialValue = value;
        }

        public Int64 Get()
        {
            Console.WriteLine("Get: " + m_InitialValue);
            return m_InitialValue;
        }

        public void SetDisposeText(string file, string text)
        {
            m_File = file;
            m_DisposeText = text;
        }

        public void Dispose()
        {
            Console.WriteLine("Dispose");
            File.WriteAllText(m_File, m_DisposeText);
        }
    }

    public sealed class ProxyClass : IRemoteClass
    {
        private IMultiProcessWorker m_ProcessWorker = ProcessWorker.Create<RemoteClass>();
        private int defaultTimeOut = 50000;

        public void Set(Int64 value)
        {
            m_ProcessWorker.ExecuteWait(Set, value, defaultTimeOut);
        }

        public Int64 Get()
        {
            return m_ProcessWorker.ExecuteWait(Get, defaultTimeOut);
        }

        public void SetDisposeText(string file, string text)
        {
            m_ProcessWorker.ExecuteWait(SetDisposeText, file, text, defaultTimeOut);
        }

        public void Dispose()
        {
            m_ProcessWorker?.Dispose();
            m_ProcessWorker = null;
        }
    }

    [TestFixture]
    public class ProcessWorkerHostedObjectTest
    {
        [Test]
        public void TestHostedObject()
        {
            var disposeFile = Path.GetTempFileName();
            var disposeText = $"Worker Dispose Test {DateTime.UtcNow}";

            using (IRemoteClass proxyClass = new ProxyClass())
            {
                var result = proxyClass.Get();
                Assert.AreEqual(1000, result);

                proxyClass.Set(5000);

                var result2 = proxyClass.Get();
                Assert.AreEqual(5000, result2);

                proxyClass.SetDisposeText(disposeFile, disposeText);
            }

            var disposeResult = File.ReadAllText(disposeFile);
            Assert.AreEqual(disposeText, disposeResult);
        }
    }
}
