using MultiProcessWorker.Private.Helper;
using MultiProcessWorker.Public.WorkItems;
using NUnit.Framework;

namespace MultiProcessWorker.Test.Private.Helper
{
    [TestFixture]
    public class ReflectionHelperTest
    {
        [Test]
        public void GetMethodInfoTest()
        {
            var workCommand = WorkCommand.Create(typeof(ReflectionHelperTest), nameof(StaticExample));
            var info = workCommand.GetMethodInfo();

            Assert.NotNull(info);
            Assert.AreEqual(typeof(ReflectionHelperTest), info.DeclaringType);
            Assert.AreEqual(nameof(StaticExample), info.Name);
        }

        [Test]
        public void GetMethodInfoFailTest()
        {
            var workCommand = WorkCommand.Create(typeof(ReflectionHelperTest), nameof(NoneStaticExample));
            var info = workCommand.GetMethodInfo();

            Assert.IsNull(info);
        }

        [Test]
        public void CreateInstanceTest()
        {
            var someClassObject = typeof(SomeClass).CreateInstance();
            Assert.NotNull(someClassObject);

            var someClass = someClassObject as SomeClass;
            Assert.NotNull(someClass);

            Assert.AreEqual(SomeClass.RefValue, someClass.Value);
        }

        [Test]
        public void ExecuteTest()
        {
            var methodInfo = typeof(ReflectionHelperTest).GetMethod(nameof(StaticExample));

            var result = methodInfo.Execute();
            Assert.AreEqual("Ok", result);

            SomeClass someClass = new SomeClass();
            var methodInfo2 = typeof(SomeClass).GetMethod(nameof(SomeClass.SetSome));

            var result2 = methodInfo2.Execute(someClass);
            Assert.AreEqual("Some", result2);
            Assert.AreEqual("Some", someClass.Value);
        }

        public static string StaticExample()
        {
            return "Ok";
        }

        public string NoneStaticExample()
        {
            return "No";
        }
    }

    public class SomeClass
    {
        public const string RefValue = "Initial";

        public string Value { get; private set; }

        public string SetSome()
        {
            Value = "Some";
            return Value;
        }

        public SomeClass()
        {
            Value = RefValue;
        }
    }
}
