namespace CoilCut.Shared;

public static class IntegrationMessageTypes
{
    public const string PlcRecipeLoaded = "plc-recipe-loaded";
    public const string MesCutPlanSent = "mes-cut-plan-sent";
    public const string ProductionCompleted = "production-completed";
    public const string PlcBenchmarkCompleted = "plc-benchmark-completed";
}

public sealed record PlcRecipeMessage(
    string WorkOrder,
    string CoilCode,
    decimal CoilWidthMm,
    decimal StripWidthMm,
    int ActiveBlades,
    decimal SpeedSetpointMMin,
    decimal TargetLengthM,
    decimal ScrapMm);

public sealed record MesCutPlanMessage(
    string OrderCode,
    string CoilId,
    decimal MotherCoilWidthMm,
    decimal StripWidthMm,
    int ProducedStrips,
    decimal ScrapWidthMm,
    decimal EfficiencyPercent);

public sealed record ProductionCompletedMessage(
    string WorkOrder,
    string CoilCode,
    decimal ProducedLengthM,
    decimal ScrapMm);

public sealed record PlcBenchmarkCompletedMessage(
    int Iterations,
    int PlcCount,
    int MeasurementCount,
    string ReportPath);
