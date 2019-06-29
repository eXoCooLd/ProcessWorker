using MultiProcessWorker.Public.Interfaces;
using NUnit.Framework;
using System;
using System.IO;

namespace MultiProcessWorker.Test
{
    public enum ObjectEnum
    {
        Default = 0,
        One,
        Two,
        Something
    }

    public enum TransportGenericEnum
    {
        One = 1,
        Two = 2,
        Three = 3
    }

    public interface IRemoteClass : IDisposable
    {
        void Set(Int64 value);
        Int64 Get();

        void SetObjectEnum(ObjectEnum enumValue);
        ObjectEnum GetObjectEnum();

        void SetDisposeText(string file, string text);

        string TransportEnumValue<TEnum>(TEnum enumValue) where TEnum : struct, IConvertible;
    }

    public sealed class RemoteClass : IRemoteClass
    {
        private Int64 m_InitialValue;
        private ObjectEnum m_ObjectEnum;
        private string m_File;
        private string m_DisposeText;

        public string TransportEnumValue<TEnum>(TEnum enumValue) where TEnum : struct, IConvertible
        {
            // https://stackoverflow.com/questions/6703180/generic-method-enum-to-string-conversion
            return Enum.GetName(typeof(TEnum), enumValue);
        }

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

        public void SetObjectEnum(ObjectEnum enumValue)
        {
            m_ObjectEnum = enumValue;
        }

        public ObjectEnum GetObjectEnum()
        {
            return m_ObjectEnum;
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

        public string TransportEnumValue<TEnum>(TEnum enumValue) where TEnum : struct, IConvertible
        {
            return m_ProcessWorker.ExecuteWait(TransportEnumValue, enumValue);
        }


        public void Set(Int64 value)
        {
            m_ProcessWorker.ExecuteWait(Set, value, defaultTimeOut);
        }

        public Int64 Get()
        {
            return m_ProcessWorker.ExecuteWait(Get, defaultTimeOut);
        }

        public void SetObjectEnum(ObjectEnum enumValue)
        {
            m_ProcessWorker.ExecuteWait(SetObjectEnum, enumValue, defaultTimeOut);
        }

        public ObjectEnum GetObjectEnum()
        {
            return m_ProcessWorker.ExecuteWait(GetObjectEnum, defaultTimeOut);
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

                var enumValue = ObjectEnum.Two;
                proxyClass.SetObjectEnum(enumValue);

                var enumResult = proxyClass.GetObjectEnum();
                Assert.AreEqual(enumValue, enumResult);

                proxyClass.SetDisposeText(disposeFile, disposeText);

                var returnedEnumString = proxyClass.TransportEnumValue<TransportGenericEnum>(TransportGenericEnum.One);

                // https://stackoverflow.com/questions/6703180/generic-method-enum-to-string-conversion
                var stringifiedEnumValue = Enum.GetName(typeof(TransportGenericEnum), TransportGenericEnum.One);

                Assert.AreEqual(stringifiedEnumValue, returnedEnumString);
            }

            var disposeResult = File.ReadAllText(disposeFile);
            Assert.AreEqual(disposeText, disposeResult);
        }
    }
}
