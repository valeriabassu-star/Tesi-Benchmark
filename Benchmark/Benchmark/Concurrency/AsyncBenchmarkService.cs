using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace PLC_Benchmark_Test.Concurrency
{
    public class AsyncBenchmarkService
    {
        public async Task<BenchmarkResult> RunAsync(int plcCount = 1, int iteration = 1)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            Task[] operations = Enumerable
                .Range(0, plcCount)
                .Select(_ => SimulatePlcCycleAsync())
                .ToArray();

            await Task.WhenAll(operations);

            stopwatch.Stop();

            return new BenchmarkResult
            {
                TestName = "Prova asincrona con async/await",
                ExecutionTime = stopwatch.ElapsedMilliseconds,
                Iteration = iteration,
                PlcCount = plcCount
            };
        }

        private static async Task SimulatePlcCycleAsync()
        {
            await Task.Delay(100); // connessione PLC
            await Task.Delay(50);  // lettura dati
            await Task.Delay(80);  // scrittura database
            await Task.Delay(10);  // elaborazione dati
        }
    }
}
