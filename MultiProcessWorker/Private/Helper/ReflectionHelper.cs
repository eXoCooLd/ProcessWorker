#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// MIT License
// Copyright(c) 2018 Andre Wehrli

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// --------------------------------------------------------------------------------------------------------------------
#endregion Copyright

#region Used Namespaces
using MultiProcessWorker.Public.WorkItems;
using System;
using System.Reflection;
#endregion Used Namespaces

namespace MultiProcessWorker.Private.Helper
{
    /// <summary>
    /// Reflection Helper
    /// </summary>
    internal static class ReflectionHelper
    {
        /// <summary>
        /// Error message for exceptions in the check method
        /// </summary>
        private const string ActionInvalidException = "Only public static methods allowed!";

        /// <summary>
        /// Get the MethodInfo from a WorkCommand
        /// </summary>
        /// <param name="workCommand"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static MethodInfo GetMethodInfo(this WorkCommand workCommand, Type type = null)
        {
            if (workCommand == null)
            {
                return null;
            }

            if (workCommand.ParameterTypes?.Length > 0 && workCommand.ParameterTypes?.Length == workCommand.Parameter?.Length)
            {
                for (int i = 0; i < workCommand.ParameterTypes.Length; i++)
                {
                    var parameterType = workCommand.ParameterTypes[i];
                    var parameterValue = workCommand.Parameter[i];

                    if (parameterValue.GetType() != parameterType)
                    {
                        var exceptionText = $"Parameter {i} Type is: {parameterValue.GetType()} but should be {parameterType}";
                        throw new ArgumentException(exceptionText);
                    }
                }
            }

            var parameterTypes = workCommand.ParameterTypes ?? new Type[0];

            if (type != null)
            {
                var methodInfo = workCommand.Type.GetMethod(workCommand.Method, BindingFlags.Public | BindingFlags.InvokeMethod | BindingFlags.Instance, null, CallingConventions.Any, parameterTypes, null);
                if (methodInfo != null)
                {
                    return methodInfo;
                }
            }

            var methodInfoStatic = workCommand.Type.GetMethod(workCommand.Method, BindingFlags.Static | BindingFlags.Public | BindingFlags.InvokeMethod, null, CallingConventions.Any, parameterTypes, null);
            if (methodInfoStatic != null)
            {
                return methodInfoStatic;
            }

            return null;
        }

        /// <summary>
        /// Check if the MethodInfo is ok
        /// </summary>
        /// <param name="memberInfo"></param>
        /// <param name="throwException"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Execute a workjob
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <param name="hostedObject"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static object Execute(this MethodInfo methodInfo, object hostedObject = null, object[] parameter = null)
        {
            if (methodInfo == null)
            {
                return null;
            }

            if (methodInfo.IsStatic)
            {
                return methodInfo.Invoke(null, parameter);
            }

            if (hostedObject != null)
            {
                return methodInfo.Invoke(hostedObject, parameter);
            }

            return null;
        }

        public static object CreateInstance(this Type type)
        {
            object instance = Activator.CreateInstance(type);

            return instance;
        }
    }
}