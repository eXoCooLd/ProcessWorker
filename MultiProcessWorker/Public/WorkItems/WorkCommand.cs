﻿#region Copyright
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
using System;
#endregion Used Namespaces

namespace MultiProcessWorker.Public.WorkItems
{
    /// <summary>
    /// Work Command for the remote process
    /// </summary>
    public class WorkCommand
    {
        /// <summary>
        /// Workjob ID
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Type that declares the Method
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// Name of the method that should be started
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// Parameter used for the method start
        /// </summary>
        public object[] Parameter { get; set; }

        /// <summary>
        /// Parameter Types for the method start
        /// </summary>
        public Type[] ParameterTypes { get; set; }

        /// <summary>
        /// To String for Outputs
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{nameof(Guid)}: {Guid} {nameof(Type)}: {Type} {nameof(Method)}: {Method} {nameof(Parameter)}: {Parameter}";
        }

        /// <summary>
        /// Create a new WorkCommand
        /// </summary>
        /// <param name="type"></param>
        /// <param name="method"></param>
        /// <param name="parameter"></param>
        /// <param name="parameterTypes"></param>
        /// <returns></returns>
        public static WorkCommand Create(Type type, string method, object[] parameter = null, Type[] parameterTypes = null)
        {
            return new WorkCommand
                        {
                            Guid = Guid.NewGuid(),
                            Type = type,
                            Method = method,
                            Parameter = parameter,
                            ParameterTypes = parameterTypes
            };
        }
    }
}