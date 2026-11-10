using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace PLC_Benchmark_Test.Concurrency
{
    public class ThreadBenchmarkService
    {
        public BenchmarkResult Run(int plcCount = 1, int iteration = 1)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            List<Thread> threads = new List<Thread>();

            for (int i = 0; i < plcCount; i++)
            {
                Thread thread = new Thread(SimulatePlcCycle);
                threads.Add(thread);
                thread.Start();
            }

            foreach (Thread thread in threads)
            {
                thread.Join();
            }

            stopwatch.Stop();

            return new BenchmarkResult
            {
                TestName = "Prova con thread",
                ExecutionTime = stopwatch.ElapsedMilliseconds,
                Iteration = iteration,
                PlcCount = plcCount
            };
        }

        private static void SimulatePlcCycle()
        {
            Thread.Sleep(100); // connessione PLC
            Thread.Sleep(50);  // lettura dati
            Thread.Sleep(80);  // scrittura database
            Thread.Sleep(10);  // elaborazione dati
        }
    }
}
