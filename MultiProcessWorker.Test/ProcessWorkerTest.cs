using MultiProcessWorker.Public.Exceptions;
using NUnit.Framework;
using System;
using System.Threading;

namespace MultiProcessWorker.Test
{
    [TestFixture]
    public class ProcessWorkerTest
    {
        private const string TestData = "Test123";

        #region Tests

        [Test]
        public void PassEnumValueOne()
        {
            Assert.Throws<ProcessWorkerRemoteException>(() =>
            {
                ProcessWorker.RunAndWait(AssertGivenEnumValue, TestTheEnums.TestValueTwo);
            });
        }

        [Test]
        public void PassEnumValueTwo()
        {
            Assert.DoesNotThrow(() =>
            {
                ProcessWorker.RunAndWait(AssertGivenEnumValue, TestTheEnums.TestValueOne);
            });
        }

        [Test]
        public void ProcessWorkerThrowingExceptionInVoidMethod()
        {
            Assert.Throws<ProcessWorkerRemoteException>(() =>
            {
                ProcessWorker.RunAndWait(VoidMethodWhichThrows);
            });
        }

        [Test]
        public void ProcessWorkerThrowingExceptionInIntMethod()
        {
            Assert.Throws<ProcessWorkerRemoteException>(() =>
            {
                ProcessWorker.RunAndWait(IntMethodWhichThrows);
            });
        }

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
        public void ProcessWorkerRemoteAssertion()
        {
            var result = ProcessWorker.RunAndWait(RemoteAssertion, 42);
            Assert.AreEqual(42, result);

            Assert.Throws<ProcessWorkerRemoteException>(() => ProcessWorker.RunAndWait(RemoteAssertion, 32));
        }

        [Test]
        public void ProcessWorkerWithIntParameterTest()
        {
            const int i1 = 1337;
            var data0 = ProcessWorker.RunAndWait(RemoteExecuteInt1, i1);
            Assert.AreEqual(i1 * 2, data0);
        }

        [Test]
        public void ProcessWorkerWithParametersOkTest()
        {
            const string p1 = "1";
            var data1 = ProcessWorker.RunAndWait(RemoteExeute1, p1);
            Assert.AreEqual(TestData + p1, data1);

            const string p2 = "2";
            var data2 = ProcessWorker.RunAndWait(RemoteExeute2, p1, p2);
            Assert.AreEqual(TestData + p1 + p2, data2);

            const string p3 = "3";
            var data3 = ProcessWorker.RunAndWait(RemoteExeute3, p1, p2, p3);
            Assert.AreEqual(TestData + p1 + p2 + p3, data3);

            const string p4 = "4";
            var data4 = ProcessWorker.RunAndWait(RemoteExeute4, p1, p2, p3, p4);
            Assert.AreEqual(TestData + p1 + p2 + p3 + p4, data4);

            const string p5 = "5";
            var data5 = ProcessWorker.RunAndWait(RemoteExeute5, p1, p2, p3, p4, p5);
            Assert.AreEqual(TestData + p1 + p2 + p3 + p4 + p5, data5);

            const string p6 = "6";
            var data6 = ProcessWorker.RunAndWait(RemoteExeute6, p1, p2, p3, p4, p5, p6);
            Assert.AreEqual(TestData + p1 + p2 + p3 + p4 + p5 + p6, data6);

            const string p7 = "7";
            var data7 = ProcessWorker.RunAndWait(RemoteExeute7, p1, p2, p3, p4, p5, p6, p7);
            Assert.AreEqual(TestData + p1 + p2 + p3 + p4 + p5 + p6 + p7, data7);

            const string p8 = "8";
            var data8 = ProcessWorker.RunAndWait(RemoteExeute8, p1, p2, p3, p4, p5, p6, p7, p8);
            Assert.AreEqual(TestData + p1 + p2 + p3 + p4 + p5 + p6 + p7 + p8, data8);

            const string p9 = "9";
            var data9 = ProcessWorker.RunAndWait(RemoteExeute9, p1, p2, p3, p4, p5, p6, p7, p8, p9);
            Assert.AreEqual(TestData + p1 + p2 + p3 + p4 + p5 + p6 + p7 + p8 + p9, data9);

            const string p10 = "A";
            var data10 = ProcessWorker.RunAndWait(RemoteExeute10, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10);
            Assert.AreEqual(TestData + p1 + p2 + p3 + p4 + p5 + p6 + p7 + p8 + p9 + p10, data10);
        }

        [Test]
        public void ProcessWorkerWithReturnTest()
        {
            ProcessWorker.RunAndWait(RemoteExecuteNoReturn, 1000);
        }

        [Test]
        public void ConverterTest()
        {
            var enumResult = ProcessWorker.RunAndWait(RemoteEnum);
            Assert.AreEqual(TestEnum.Leet, enumResult);

            var intResult = ProcessWorker.RunAndWait(RemoteInt);
            Assert.AreEqual(12345, intResult);

        }
        #endregion Tests

        #region Test Methods

        public static void AssertGivenEnumValue(TestTheEnums anEnumValue)
        {
            Assert.AreEqual(TestTheEnums.TestValueOne, anEnumValue);
        }

        public static int IntMethodWhichThrows()
        {
            throw new Exception("THIS_EXCEPTION_FROM_INT_METHOD_SHOULD_BE_FORWARDED");
        }

        public static void VoidMethodWhichThrows()
        {
            throw new Exception("THIS_EXCEPTION_FROM_VOID_METHOD_SHOULD_BE_FORWARDED");
        }

        public string RemoteFailExecute()
        {
            return "nope not public static!";
        }

        public static string RemoteExecute()
        {
            return TestData;
        }

        public static string RemoteExceptionExecute()
        {
            throw new InvalidOperationException("Remote Exception!");
        }

        public static int RemoteAssertion(int value)
        {
            const int result = 42;
            Assert.AreEqual(result, value, $"{nameof(value)} is not {result}");

            return result;
        }

        public static int RemoteExecuteInt1(int value)
        {
            return value * 2;
        }

        public static string RemoteExeute1(string p1)
        {
            return TestData + p1;
        }

        public static string RemoteExeute2(string p1, string p2)
        {
            return TestData + p1 + p2;
        }

        public static string RemoteExeute3(string p1, string p2, string p3)
        {
            return TestData + p1 + p2 + p3;
        }

        public static string RemoteExeute4(string p1, string p2, string p3, string p4)
        {
            return TestData + p1 + p2 + p3 + p4;
        }

        public static string RemoteExeute5(string p1, string p2, string p3, string p4, string p5)
        {
            return TestData + p1 + p2 + p3 + p4 + p5;
        }

        public static string RemoteExeute6(string p1, string p2, string p3, string p4, string p5, string p6)
        {
            return TestData + p1 + p2 + p3 + p4 + p5 + p6;
        }

        public static string RemoteExeute7(string p1, string p2, string p3, string p4, string p5, string p6, string p7)
        {
            return TestData + p1 + p2 + p3 + p4 + p5 + p6 + p7;
        }

        public static string RemoteExeute8(string p1, string p2, string p3, string p4, string p5, string p6, string p7, string p8)
        {
            return TestData + p1 + p2 + p3 + p4 + p5 + p6 + p7 + p8;
        }

        public static string RemoteExeute9(string p1, string p2, string p3, string p4, string p5, string p6, string p7, string p8, string p9)
        {
            return TestData + p1 + p2 + p3 + p4 + p5 + p6 + p7 + p8 + p9;
        }

        public static string RemoteExeute10(string p1, string p2, string p3, string p4, string p5, string p6, string p7, string p8, string p9, string p10)
        {
            return TestData + p1 + p2 + p3 + p4 + p5 + p6 + p7 + p8 + p9 + p10;
        }

        public static void RemoteExecuteNoReturn()
        {
            Thread.Sleep(5);
        }

        public static TestEnum RemoteEnum()
        {
            return TestEnum.Leet;
        }

        public static int RemoteInt()
        {
            return 12345;
        }

        #endregion Test Methods

    }

    public enum TestEnum
    {
        Undefined = 0,
        One = 1,
        Two = 2,
        Leet = 1337
    }

    public enum TestTheEnums
    {
        TestValueOne,
        TestValueTwo
    }
}
