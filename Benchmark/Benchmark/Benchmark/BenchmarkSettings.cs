namespace PLC_Benchmark_Test.Benchmark
{
    public class BenchmarkSettings
    {
        public int Iterations { get; set; } = 10;
        public int PlcCount { get; set; } = 5;
        public string OutputDirectory { get; set; } = "Report";
    }
}
