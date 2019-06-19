using MultiProcessWorker.Public.Interfaces;
using NUnit.Framework;
using System;

namespace MultiProcessWorker.Test
{
    public interface IRemoteClass : IDisposable
    {
        void Set(Int64 value);
        Int64 Get();
    }

    public sealed class RemoteClass : IRemoteClass
    {
        private Int64 m_InitialValue;

        public RemoteClass()
        {
            m_InitialValue = 1000;
            Console.WriteLine("Initial " + m_InitialValue);
        }

        public void Set(Int64 value)
        {
            Console.WriteLine("Set: " + value);
            m_InitialValue = value;
        }

        public Int64 Get()
        {
            Console.WriteLine("Get " + m_InitialValue);
            return m_InitialValue;
        }

        public void Dispose()
        {
            // DEBUGGING
            System.Diagnostics.Debugger.Launch();

            Console.WriteLine("Dispose");
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
        //[Ignore("HostedObject not ready yet")]
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
