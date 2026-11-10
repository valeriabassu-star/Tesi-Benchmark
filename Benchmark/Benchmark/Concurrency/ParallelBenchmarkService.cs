using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PLC_Benchmark_Test.Concurrency
{
    public class ParallelBenchmarkService
    {
        public BenchmarkResult Run(int plcCount = 1, int iteration = 1)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            Parallel.ForEach(Enumerable.Range(0, plcCount), _ => SimulatePlcCycle());

            stopwatch.Stop();

            return new BenchmarkResult
            {
                TestName = "Prova con elaborazione parallela",
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
