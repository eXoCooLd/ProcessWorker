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

        public static string StaticExample()
        {
            return "Ok";
        }

        public string NoneStaticExample()
        {
            return "No";
        }
    }
}
