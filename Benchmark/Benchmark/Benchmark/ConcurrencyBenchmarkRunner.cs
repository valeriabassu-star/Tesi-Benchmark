using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PLC_Benchmark_Test.Concurrency;

namespace PLC_Benchmark_Test.Benchmark
{
    public class ConcurrencyBenchmarkRunner
    {
        public async Task<IReadOnlyList<BenchmarkResult>> RunAsync(BenchmarkSettings settings)
        {
            List<BenchmarkResult> results = new List<BenchmarkResult>();

            AsyncBenchmarkService asyncBenchmark = new AsyncBenchmarkService();
            ThreadBenchmarkService threadBenchmark = new ThreadBenchmarkService();
            ParallelBenchmarkService parallelBenchmark = new ParallelBenchmarkService();

            for (int iteration = 1; iteration <= settings.Iterations; iteration++)
            {
                results.Add(await asyncBenchmark.RunAsync(settings.PlcCount, iteration));
                results.Add(threadBenchmark.Run(settings.PlcCount, iteration));
                results.Add(parallelBenchmark.Run(settings.PlcCount, iteration));
            }

            return results;
        }

        public IReadOnlyList<BenchmarkStatistics> CalculateStatistics(IReadOnlyList<BenchmarkResult> results)
        {
            return results
                .GroupBy(result => result.TestName)
                .Select(group => BenchmarkStatistics.From(
                    group.Key,
                    group.Select(result => result.ExecutionTime).ToList()))
                .ToList();
        }
    }
}
