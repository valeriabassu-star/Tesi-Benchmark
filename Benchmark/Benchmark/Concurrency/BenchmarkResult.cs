namespace PLC_Benchmark_Test.Concurrency
{
    public class BenchmarkResult
    {
        public string TestName { get; set; } = string.Empty;
        public long ExecutionTime { get; set; }
        public int Iteration { get; set; }
        public int PlcCount { get; set; }

        public override string ToString()
        {
            return $"{TestName} | iterazione {Iteration} | PLC {PlcCount}: {ExecutionTime} ms";
        }
    }
}
