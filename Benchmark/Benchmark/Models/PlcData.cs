namespace PLC_Benchmark_Test.Models
{
    // Modello dati che rappresenta le informazioni lette dal PLC.
    // Contiene i principali parametri della macchina simulata: temperatura, velocità e stato operativo
  
    public class PlcData
    {
        public int Temperature { get; set; }
        public int Speed { get; set; }
        public bool MachineStatus { get; set; }
        public double MotorCurrent { get; set; }
        public double Pressure { get; set; }
        public int ProductionCounter { get; set; }
        public bool AlarmActive { get; set; }
        public string AlarmCode { get; set; } = string.Empty;

    }
}