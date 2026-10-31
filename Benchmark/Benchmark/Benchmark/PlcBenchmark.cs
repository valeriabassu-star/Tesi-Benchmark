using System;
using System.Diagnostics;
using PLC_Benchmark_Test.Models;
using PLC_Benchmark_Test.PLC;
using PLC_Benchmark_Test.Services;

namespace PLC_Benchmark_Test.Benchmark
{
    public class PlcBenchmark
    {
        public void Run()
        {
            PlcSimulator plc = new PlcSimulator();
            DatabaseService databaseService = new DatabaseService();
            DataProcessingService processingService = new DataProcessingService();

            Stopwatch stopwatch = new Stopwatch();

            stopwatch.Restart();
            plc.Connect();
            stopwatch.Stop();
            long connectionTime = stopwatch.ElapsedMilliseconds;

            stopwatch.Restart();
            PlcData data = plc.ReadData();
            stopwatch.Stop();
            long readTime = stopwatch.ElapsedMilliseconds;

            stopwatch.Restart();
            databaseService.SaveData(data);
            stopwatch.Stop();
            long databaseWriteTime = stopwatch.ElapsedMilliseconds;

            stopwatch.Restart();
            double processedValue = processingService.ProcessData(data);
            stopwatch.Stop();
            long processingTime = stopwatch.ElapsedMilliseconds;

            Console.WriteLine($"Temperatura: {data.Temperature} °C");
            Console.WriteLine($"Velocità linea: {data.Speed} m/min");
            Console.WriteLine($"Macchina attiva: {data.MachineStatus}");
            Console.WriteLine($"Corrente motore: {data.MotorCurrent} A");
            Console.WriteLine($"Pressione: {data.Pressure} bar");
            Console.WriteLine($"Contatore produzione: {data.ProductionCounter}");
            Console.WriteLine($"Allarme attivo: {data.AlarmActive}");

            if (data.AlarmActive)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Codice allarme: {data.AlarmCode}");
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine("Codice allarme: Nessun allarme presente");
            }
            Console.WriteLine($"Valore elaborato: {processedValue}");
            Console.WriteLine();

            Console.WriteLine($"1. Tempo di connessione alla macchina: {connectionTime} ms");
            Console.WriteLine($"2. Tempo di lettura parametri: {readTime} ms");
            Console.WriteLine($"3. Tempo di scrittura dati su database: {databaseWriteTime} ms");
            Console.WriteLine($"4. Tempo di elaborazione dati: {processingTime} ms");
        }
    }
}