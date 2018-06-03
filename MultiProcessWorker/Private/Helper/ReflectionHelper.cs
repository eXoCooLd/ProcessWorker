using MultiProcessWorker.Public.WorkItems;
using System;
using System.Reflection;

namespace MultiProcessWorker.Private.Helper
{
    internal static class ReflectionHelper
    {
        private const string ActionInvalidException = "Only public static methods allowed!";

        public static MethodInfo GetMethodInfo(this WorkCommand workCommand)
        {
            var methodInfo = workCommand.Type.GetMethod(workCommand.Method, BindingFlags.Static | BindingFlags.Public | BindingFlags.InvokeMethod);

            return methodInfo;
        }

        public static bool CheckMethodInfo(this MemberInfo memberInfo, bool throwException = true)
        {
            if (memberInfo == null)
            {
                if (throwException)
                {
                    throw new ArgumentException(ActionInvalidException);
                }

                return false;
            }

            if (memberInfo.MemberType != MemberTypes.Method)
            {
                if (throwException)
                {
                    throw new ArgumentException(ActionInvalidException);
                }

                return false;
            }

            return true;
        }

        public static object DoWork(this MethodInfo methodInfo)
        {
            object result = null;

            if (methodInfo != null)
            {
                result = methodInfo.Invoke(null, null);
            }

            return result;
        }
    }
}