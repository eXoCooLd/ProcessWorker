using System;
using System.IO;

namespace MultiProcessWorker.Private.Helper
{
    internal class AssemblyHelper
    {
        public static string CurrentAssemblyPath
        {
            get
            {
                var uriBuilder = new UriBuilder(typeof(AssemblyHelper).Assembly.CodeBase);
                var path = Uri.UnescapeDataString(uriBuilder.Path);

                return Path.GetFullPath(path);
            }
        }
    }
}