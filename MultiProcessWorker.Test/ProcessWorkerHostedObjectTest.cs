using MultiProcessWorker.Public.Interfaces;
using NUnit.Framework;
using System;

namespace MultiProcessWorker.Test
{
    public interface IRemoteClass : IDisposable
    {
        void Set(int value);
        int Get();
    }

    public sealed class RemoteClass : IRemoteClass
    {
        private int m_InitialValue;

        public RemoteClass()
        {
            m_InitialValue = 1000;
        }

        public void Set(int value)
        {
            m_InitialValue = value;
        }

        public int Get()
        {
            return m_InitialValue;
        }

        public void Dispose()
        {

        }
    }

    public sealed class ProxyClass : IRemoteClass
    {
        private IMultiProcessWorker m_ProcessWorker = ProcessWorker.Create<RemoteClass>();
        private int defaultTimeOut = 5000;

        public void Set(int value)
        {
            m_ProcessWorker.ExecuteWait(Set, value, defaultTimeOut);
        }

        public int Get()
        {
            return m_ProcessWorker.ExecuteWait(Get, defaultTimeOut);
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
        [Ignore("HostedObject not ready yet")]
        public void TestHostedObject()
        {
            using (IRemoteClass proxyClass = new ProxyClass())
            {
                var result = proxyClass.Get();
                Assert.AreEqual(1000, result);

                proxyClass.Set(5000);

                var result2 = proxyClass.Get();
                Assert.AreEqual(5000, result2);
            }
        }
    }
}
