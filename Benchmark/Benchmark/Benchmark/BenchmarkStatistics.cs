using System;
using System.Collections.Generic;
using System.Linq;

namespace PLC_Benchmark_Test.Benchmark
{
    public class BenchmarkStatistics
    {
        public string TestName { get; set; } = string.Empty;
        public int Iterations { get; set; }
        public double AverageMilliseconds { get; set; }
        public long MinimumMilliseconds { get; set; }
        public long MaximumMilliseconds { get; set; }
        public double StandardDeviation { get; set; }

        public static BenchmarkStatistics From(string testName, IReadOnlyList<long> measurements)
        {
            if (measurements.Count == 0)
            {
                throw new ArgumentException("Il benchmark richiede almeno una misurazione.", nameof(measurements));
            }

            double average = measurements.Average();
            double variance = measurements
                .Select(value => Math.Pow(value - average, 2))
                .Average();

            return new BenchmarkStatistics
            {
                TestName = testName,
                Iterations = measurements.Count,
                AverageMilliseconds = Math.Round(average, 2),
                MinimumMilliseconds = measurements.Min(),
                MaximumMilliseconds = measurements.Max(),
                StandardDeviation = Math.Round(Math.Sqrt(variance), 2)
            };
        }

        public override string ToString()
        {
            return $"{TestName}: media {AverageMilliseconds} ms | min {MinimumMilliseconds} ms | max {MaximumMilliseconds} ms | dev. std {StandardDeviation} ms";
        }
    }
}
