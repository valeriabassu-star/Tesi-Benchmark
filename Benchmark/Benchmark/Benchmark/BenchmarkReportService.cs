using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using PLC_Benchmark_Test.Concurrency;

namespace PLC_Benchmark_Test.Benchmark
{
    public class BenchmarkReportService
    {
        public string SaveCsv(
            BenchmarkSettings settings,
            IReadOnlyList<BenchmarkResult> measurements,
            IReadOnlyList<BenchmarkStatistics> statistics)
        {
            Directory.CreateDirectory(settings.OutputDirectory);

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
            string filePath = Path.Combine(settings.OutputDirectory, $"report_prestazioni_{timestamp}.csv");

            using StreamWriter writer = new StreamWriter(filePath);

            writer.WriteLine("Sezione; Test; Iterazione; TempoMs; NumeroPlc; MediaMs; MinMs; MaxMs; DeviazioneStandardMs");

            foreach (BenchmarkResult measurement in measurements)
            {
                writer.WriteLine(string.Join(";",
                    "Misurazione",
                    measurement.TestName,
                    measurement.Iteration,
                    measurement.ExecutionTime,
                    measurement.PlcCount,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty));
            }

            foreach (BenchmarkStatistics item in statistics)
            {
                writer.WriteLine(string.Join(";",
                    "Statistiche",
                    item.TestName,
                    item.Iterations,
                    string.Empty,
                    settings.PlcCount,
                    item.AverageMilliseconds.ToString(CultureInfo.InvariantCulture),
                    item.MinimumMilliseconds,
                    item.MaximumMilliseconds,
                    item.StandardDeviation.ToString(CultureInfo.InvariantCulture)));
            }

            return Path.GetFullPath(filePath);
        }
    }
}
