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
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

#endregion Used Namespaces

namespace MultiProcessWorker.Private.ProcessData
{
    internal class ProcessArguments
    {

        #region Constants and Enums

        private const string KeyPrefix = @"\";
        private const string KeyValueSplitter = "=";
        private const string ArgumentSplitter = " ";

        private const string IpcServerNameKey = "S";
        private const string IpcClientNameKey = "C";
        private const string IpcProcessNameKey = "P";
        private const string IpcParentProgramKey = "X";
        private const string IpcRemoteTypeKey = "R";
        private const string IpcAssemblyKey = "A";

        private const string ClientPostfix = "_Client";
        private const string ServerPostfix = "_Server";
        private const string ProcessPostfix = "_Process";

        #endregion Constants and Enums

        #region Fields

        private readonly IDictionary<string, string> m_Arguments;
        
        #endregion Fields

        #region Construction/Destruction/Initialisation

        private ProcessArguments()
        {
            m_Arguments = new Dictionary<string, string>();
        }

        private ProcessArguments(string[] args)
        {
            m_Arguments = args.ToDictionary(CreateKey, CreateValue);
        }

        #endregion Construction/Destruction/Initialisation

        #region Properties

        public string this[string key]
        {
            get { return m_Arguments[key]; }
            set { m_Arguments[key] = value; }
        }

        public string IpcServerName
        {
            get { return this[IpcServerNameKey]; }
            set { this[IpcServerNameKey] = value; }
        }

        public string IpcClientName
        {
            get { return this[IpcClientNameKey]; }
            set { this[IpcClientNameKey] = value; }
        }

        public string IpcProcessName
        {
            get { return this[IpcProcessNameKey]; }
            set { this[IpcProcessNameKey] = value; }
        }

        public int IpcParentProgramPid
        {
            get
            {
                int pid;
                int.TryParse(this[IpcParentProgramKey], out pid);
                return pid;
            }
            set { this[IpcParentProgramKey] = value.ToString(); }
        }

        public Assembly IpcAssembly
        {
            get
            {
                var assemblyName = this[IpcAssemblyKey];
                return Assembly.Load(assemblyName);
            }
            set
            {
                var assemblyName = value?.GetName().Name;
                this[IpcAssemblyKey] = assemblyName;
            }
        }

        public Type IpcRemoteType
        {
            get
            {
                var typeString = this[IpcRemoteTypeKey];
                return !string.IsNullOrEmpty(typeString) ? IpcAssembly?.GetType(typeString) : null;
            }
            set
            {
                var typeString = value?.FullName;
                this[IpcRemoteTypeKey] = typeString;
            }
        }

        public bool IsValid => m_Arguments.ContainsKey(IpcServerNameKey) &&
                               m_Arguments.ContainsKey(IpcClientNameKey) &&
                               m_Arguments.ContainsKey(IpcProcessNameKey) &&
                               m_Arguments.ContainsKey(IpcParentProgramKey);

        #endregion Properties

        #region Factories

        public static ProcessArguments Create(string[] args)
        {
            var processArguments = new ProcessArguments(args);
            return processArguments.IsValid ? processArguments : null;
        }

        public static ProcessArguments Create(string ipcName, Type remoteType = null)
        {
            var processArguments = new ProcessArguments
                                    {
                                        IpcClientName = CreateIpcClientName(ipcName),
                                        IpcServerName = CreateIpcServerName(ipcName),
                                        IpcProcessName = CreateIpcProcessName(ipcName),
                                        IpcParentProgramPid = GetParentProgramPid(),
                                        IpcAssembly = remoteType?.Assembly,
                                        IpcRemoteType = remoteType
            };
            return processArguments;
        }

        #endregion Factories

        #region Public

        public string[] ToStringArray()
        {
            return m_Arguments.Select(kv => KeyPrefix + kv.Key + KeyValueSplitter + kv.Value).ToArray();
        }

        public override string ToString()
        {
            return string.Join(ArgumentSplitter, ToStringArray());
        }

        #endregion Public

        #region Internal

        internal static string CreateIpcClientName(string ipcName)
        {
            return ipcName + ClientPostfix;
        }

        internal static string CreateIpcServerName(string ipcName)
        {
            return ipcName + ServerPostfix;
        }

        internal static string CreateIpcProcessName(string ipcName)
        {
            return ipcName + ProcessPostfix;
        }

        internal static int GetParentProgramPid()
        {
            return Process.GetCurrentProcess().Id;
        }

        #endregion Internal

        #region Private

        private static string CreateKey(string input)
        {
            var length = input.IndexOf(KeyValueSplitter, StringComparison.Ordinal);
            return length <= 0 ? string.Empty : input.Substring(0, length).Replace(KeyPrefix, string.Empty).Trim();
        }

        private static string CreateValue(string input)
        {
            var length = input.IndexOf(KeyValueSplitter, StringComparison.Ordinal) + KeyValueSplitter.Length;
            return length >= input.Length ? string.Empty : input.Substring(length).Trim();
        }

        #endregion Private

    }
}