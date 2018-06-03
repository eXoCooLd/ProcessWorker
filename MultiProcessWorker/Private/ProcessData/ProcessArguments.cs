using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MultiProcessWorker.Private.ProcessData
{
    internal class ProcessArguments
    {
        private const string KeyPrefix = "-";
        private const string KeyValueSplitter = "=";
        private const string ArgumentSplitter = " ";

        private const string IpcServerNameKey = "S";
        private const string IpcClientNameKey = "C";
        private const string IpcProcessNameKey = "P";
        private const string IpcParentProgramKey = "X";

        private const string ClientPostfix = "_Client";
        private const string ServerPostfix = "_Server";
        private const string ProcessPostfix = "_Process";

        private readonly IDictionary<string, string> m_Arguments;

        private ProcessArguments()
        {
            m_Arguments = new Dictionary<string, string>();
        }

        private ProcessArguments(string[] args)
        {
            m_Arguments = args.ToDictionary(CreateKey, CreateValue);
        }

        public string this[string key]
        {
            get => m_Arguments[key];
            set => m_Arguments[key] = value;
        }

        public string IpcServerName
        {
            get => this[IpcServerNameKey];
            set => this[IpcServerNameKey] = value;
        }

        public string IpcClientName
        {
            get => this[IpcClientNameKey];
            set => this[IpcClientNameKey] = value;
        }

        public string IpcProcessName
        {
            get => this[IpcProcessNameKey];
            set => this[IpcProcessNameKey] = value;
        }

        public int IpcParentProgramPid
        {
            get
            {
                int.TryParse(this[IpcParentProgramKey], out int pid);
                return pid;
            }
            set => this[IpcParentProgramKey] = value.ToString();
        }

        public bool IsValid => m_Arguments.ContainsKey(IpcServerNameKey) &&
                               m_Arguments.ContainsKey(IpcClientNameKey) &&
                               m_Arguments.ContainsKey(IpcProcessNameKey) &&
                               m_Arguments.ContainsKey(IpcParentProgramKey);

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

        public static ProcessArguments Create(string[] args)
        {
            var processArguments = new ProcessArguments(args);
            return processArguments.IsValid ? processArguments : null;
        }

        public static ProcessArguments Create(string ipcName)
        {
            var processArguments = new ProcessArguments
                                    {
                                        IpcClientName = CreateIpcClientName(ipcName),
                                        IpcServerName = CreateIpcServerName(ipcName),
                                        IpcProcessName = CreateIpcProcessName(ipcName),
                                        IpcParentProgramPid = GetParentProgramPid()
                                    };
            return processArguments;
        }

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

        public string[] ToStringArray()
        {
            return m_Arguments.Select(kv => KeyPrefix + kv.Key + KeyValueSplitter + kv.Value).ToArray();
        }

        public override string ToString()
        {
            return string.Join(ArgumentSplitter, ToStringArray());
        }
    }
}