using MultiProcessWorker.Private.Helper;
using NUnit.Framework;
using System;
using System.IO;

namespace MultiProcessWorker.Test.Private.Helper
{
    [TestFixture]
    public class AssemblyHelperTest
    {
        [Test]
        public void CurrentAssemblyPathTest()
        {
            var path = AssemblyHelper.CurrentAssemblyPath;

            Assert.IsTrue(File.Exists(path));
            Assert.AreEqual(GetAssemblyPath(typeof(AssemblyHelper)), path);
        }

        public static string GetAssemblyPath(Type type)
        {
                var uriBuilder = new UriBuilder(type.Assembly.CodeBase);
                var path = Uri.UnescapeDataString(uriBuilder.Path);

                return Path.GetFullPath(path);
        }
    }
}
