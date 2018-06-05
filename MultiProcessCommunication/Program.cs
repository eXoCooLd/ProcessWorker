using System;
using System.Diagnostics;
using System.Threading;
using MultiProcessWorker;
using MultiProcessWorker.Public.Exceptions;

namespace MultiProcessCommunication
{
    /// <summary>
    /// Example program
    /// </summary>
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Start MultiProcessWorkerClient Demo");

            using (var processWorker = ProcessWorker.Create())
            {
                var resultGuid = processWorker.Execute(ExecuteThisNow);

                Console.WriteLine("key to send");
                Console.ReadKey();

                var result = processWorker.ExecuteWait(ExecuteMeNow);

                Console.WriteLine(result);

                Console.WriteLine("key to send");
                Console.ReadKey();

                if (processWorker.IsDataReady(resultGuid))
                {
                    var result2 = processWorker.GetResult<string>(resultGuid);
                    Console.WriteLine(result2);
                }

                Console.WriteLine("key to exit");
                Console.ReadKey();
            }

            try
            {
                ProcessWorker.RunAndWait(ExecuteFuckedUp);
            }
            catch (ProcessWorkerRemoteException e)
            {
                Console.WriteLine(e);
            }
        }

        /// <summary>
        /// Workjob with simulated delay
        /// </summary>
        /// <returns></returns>
        public static string ExecuteMeNow()
        {
            Console.WriteLine("Starting work...");
            Thread.Sleep(5000);

            var text = "Yea from: " + Process.GetCurrentProcess().Id;
            Console.WriteLine(text);
            return text;
        }

        /// <summary>
        /// Workjob
        /// </summary>
        /// <returns></returns>
        public static string ExecuteThisNow()
        {
            var text = "Blabla from: " + Process.GetCurrentProcess().Id;
            Console.WriteLine(text);
            return text;
        }

        /// <summary>
        /// Exception in the remote program
        /// </summary>
        /// <returns></returns>
        public static string ExecuteFuckedUp()
        {
            throw new InvalidOperationException("FuckedUp by: " + Process.GetCurrentProcess().Id);
        }
    }
}