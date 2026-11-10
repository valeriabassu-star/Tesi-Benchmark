using System;
using System.Threading;
using PLC_Benchmark_Test.Models;

namespace PLC_Benchmark_Test.PLC
{
    // Classe che simula il comportamento di un PLC industriale
    // Permette di simulare operazioni come la connessione alla macchina
    // e la lettura dei parametri di funzionamento

    public class PlcSimulator
    {
        private readonly Random _random = new Random();

        public void Connect()
        {
            Thread.Sleep(100);
        }

        public PlcData ReadData()
        {
            Thread.Sleep(50);

            string[] alarmCodes =
            {
        "ALM-001 Sovratemperatura motore",
        "ALM-002 Pressione insufficiente",
        "ALM-003 Sovracorrente motore",
        "ALM-004 Arresto emergenza attivo",
        "ALM-005 Errore comunicazione PLC"
    };

            bool alarmActive = _random.Next(0, 5) == 1;

            string selectedAlarm = alarmActive
                ? alarmCodes[_random.Next(alarmCodes.Length)]
                : string.Empty;

            return new PlcData
            {
                Temperature = _random.Next(20, 81),
                Speed = _random.Next(100, 501),
                MachineStatus = _random.Next(0, 2) == 1,

                MotorCurrent = Math.Round(_random.NextDouble() * 100, 2),
                Pressure = Math.Round(_random.NextDouble() * 10, 2),

                ProductionCounter = _random.Next(0, 10000),

                AlarmActive = alarmActive,
                AlarmCode = selectedAlarm
            };
        }
    }
}