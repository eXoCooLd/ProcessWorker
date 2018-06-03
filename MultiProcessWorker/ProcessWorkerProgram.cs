using MultiProcessWorker.Private.MultiProcessWorkerLogic;
using System;

namespace MultiProcessWorker
{
    internal class ProcessWorkerProgram
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Start MultiProcessWorker");

            using (var runner = MultiProcessWorkerRunner.Create(args))
            {
                runner?.Run();
            }
        }
    }
}