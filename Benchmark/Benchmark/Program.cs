using System;
using CoilCut.Shared;
using System.Threading.Tasks;
using PLC_Benchmark_Test.Benchmark;

namespace PLC_Benchmark_Test
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            JsonIntegrationBus integrationBus = new();

            Console.WriteLine("PROVA PRESTAZIONALE DEL PLC");
            Console.WriteLine("------------------");
            PrintLatestIntegrationMessages(integrationBus);

            PlcBenchmark benchmark = new PlcBenchmark();
            benchmark.Run();

            Console.WriteLine();
            Console.WriteLine("CONFRONTO TRA CONCORRENZA E ASINCRONIA");
            Console.WriteLine("-----------------------------");

            BenchmarkSettings settings = new BenchmarkSettings
            {
                Iterations = ReadInt("Numero iterazioni benchmark", 10),
                PlcCount = ReadInt("Numero PLC simulati", 5)
            };

            ConcurrencyBenchmarkRunner runner = new ConcurrencyBenchmarkRunner();
            var measurements = await runner.RunAsync(settings);
            var statistics = runner.CalculateStatistics(measurements);

            Console.WriteLine();
            Console.WriteLine("Risultati statistici:");
            foreach (var item in statistics)
            {
                Console.WriteLine(item);
            }

            BenchmarkReportService reportService = new BenchmarkReportService();
            string reportPath = reportService.SaveCsv(settings, measurements, statistics);

            Console.WriteLine();
            Console.WriteLine($"Report CSV salvato in: {reportPath}");
            integrationBus.Publish(
                IntegrationMessageTypes.PlcBenchmarkCompleted,
                "Benchmark_tesi",
                new PlcBenchmarkCompletedMessage(
                    settings.Iterations,
                    settings.PlcCount,
                    measurements.Count,
                    reportPath));

            Console.WriteLine();
            Console.WriteLine("Test completato.");
            Console.ReadLine();
        }

        private static void PrintLatestIntegrationMessages(JsonIntegrationBus integrationBus)
        {
            Console.WriteLine($"Cartella scambio JSON: {integrationBus.MessageFolder}");

            var recipe = integrationBus.ReadLatest(IntegrationMessageTypes.PlcRecipeLoaded);
            if (recipe is not null)
            {
                Console.WriteLine($"Ultima ricetta HMI ricevuta: {recipe.Payload}");
            }

            var cutPlan = integrationBus.ReadLatest(IntegrationMessageTypes.MesCutPlanSent);
            if (cutPlan is not null)
            {
                Console.WriteLine($"Ultimo piano MES ricevuto: {cutPlan.Payload}");
            }

            Console.WriteLine();
        }

        private static int ReadInt(string label, int defaultValue)
        {
            Console.Write($"{label} [{defaultValue}]: ");
            string? input = Console.ReadLine();

            if (int.TryParse(input, out int value) && value > 0)
            {
                return value;
            }

            return defaultValue;
        }
    }
}
