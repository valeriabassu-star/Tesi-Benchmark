using System;
using System.Threading;
using PLC_Benchmark_Test.Models;

namespace PLC_Benchmark_Test.Services
{
    // Servizio che simula la scrittura dei dati su un database
    // Nel progetto reale questo modulo gestirebbe l'interazione con SQL Server

    public class DatabaseService
    {
        public void SaveData(PlcData data)
        {
            Thread.Sleep(80);

            Console.WriteLine();
            Console.WriteLine("Dati salvati nel database:");
            Console.WriteLine($"Data e ora: {DateTime.Now}");
            Console.WriteLine($"Temperatura: {data.Temperature}");
            Console.WriteLine($"Velocità: {data.Speed}");
            Console.WriteLine($"Contatore produzione: {data.ProductionCounter}");
        }
    }
}
