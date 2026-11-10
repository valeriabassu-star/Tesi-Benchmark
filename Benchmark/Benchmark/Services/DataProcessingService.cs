using System;
using PLC_Benchmark_Test.Models;

namespace PLC_Benchmark_Test.Services
{
    public class DataProcessingService
    {
        public double ProcessData(PlcData data)
        {
            if (data.AlarmActive)
            {
                Console.WriteLine("Allarme macchina rilevato");
            }

            double efficiency =
                (data.Speed * data.Pressure) /
                (data.MotorCurrent + 1);

            return Math.Round(efficiency, 2);
        }
    }
}