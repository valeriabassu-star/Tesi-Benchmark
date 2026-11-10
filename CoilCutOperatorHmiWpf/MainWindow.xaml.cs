using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using CoilCut.Shared;

namespace CoilCutOperatorHmiWpf;

public partial class MainWindow : Window, INotifyPropertyChanged
{
    private readonly JsonIntegrationBus _integrationBus = new();
    private readonly DispatcherTimer _machineTimer;
    private readonly MachineControlService _machineControlService = new();

    private string _workOrder = "ORD-COIL-2026-021";
    private string _coilCode = "COIL-ZN-8842";
    private string _coilWidthText = "1250";
    private string _stripWidthText = "250";
    private string _activeBladesText = "4";
    private string _speedSetpointText = "42";
    private string _targetLengthText = "1200";
    private int _bladeLayoutColumns = 4;
    private bool _sendToMes = true;
    private MachineRunState _runState = MachineRunState.Idle;
    private decimal _actualSpeed;
    private decimal _progressMeters;
    private decimal _tensionKn;
    private decimal _scrapMm;
    private string _mesTransferStatus = "MES in attesa";
    private string _plcPayload = "Caricare una ricetta per visualizzare i dati inviati al controllo macchina.";

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;

        _machineTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(500)
        };
        _machineTimer.Tick += MachineTimer_Tick;

        LoadRecipe();
        AddMessage("Informazione", "Interfaccia operatore avviata.");
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<MachineMessage> MachineMessages { get; } = new();

    public ObservableCollection<BladePosition> BladePositions { get; } = new();

    public int BladeLayoutColumns
    {
        get => _bladeLayoutColumns;
        private set => SetField(ref _bladeLayoutColumns, value);
    }

    public string WorkOrder
    {
        get => _workOrder;
        set => SetField(ref _workOrder, value);
    }

    public string CoilCode
    {
        get => _coilCode;
        set => SetField(ref _coilCode, value);
    }

    public string CoilWidthText
    {
        get => _coilWidthText;
        set => SetField(ref _coilWidthText, value);
    }

    public string StripWidthText
    {
        get => _stripWidthText;
        set => SetField(ref _stripWidthText, value);
    }

    public string ActiveBladesText
    {
        get => _activeBladesText;
        set => SetField(ref _activeBladesText, value);
    }

    public string SpeedSetpointText
    {
        get => _speedSetpointText;
        set => SetField(ref _speedSetpointText, value);
    }

    public string TargetLengthText
    {
        get => _targetLengthText;
        set => SetField(ref _targetLengthText, value);
    }

    public bool SendToMes
    {
        get => _sendToMes;
        set => SetField(ref _sendToMes, value);
    }

    public string MachineState => _runState switch
    {
        MachineRunState.Running => "MACCHINA IN MARCIA",
        MachineRunState.Paused => "MACCHINA IN PAUSA",
        MachineRunState.Alarm => "ALLARME MACCHINA",
        _ => "MACCHINA FERMA"
    };

    public Brush MachineStateBrush => _runState switch
    {
        MachineRunState.Running => Brushes.SeaGreen,
        MachineRunState.Paused => Brushes.DarkOrange,
        MachineRunState.Alarm => Brushes.Firebrick,
        _ => Brushes.SlateGray
    };

    public Brush MachineStateBackground => _runState switch
    {
        MachineRunState.Running => new SolidColorBrush(Color.FromRgb(231, 246, 239)),
        MachineRunState.Paused => new SolidColorBrush(Color.FromRgb(255, 246, 226)),
        MachineRunState.Alarm => new SolidColorBrush(Color.FromRgb(252, 232, 232)),
        _ => new SolidColorBrush(Color.FromRgb(238, 243, 247))
    };

    public Brush MachineStateBorder => _runState switch
    {
        MachineRunState.Running => new SolidColorBrush(Color.FromRgb(174, 222, 199)),
        MachineRunState.Paused => new SolidColorBrush(Color.FromRgb(237, 196, 116)),
        MachineRunState.Alarm => new SolidColorBrush(Color.FromRgb(225, 165, 165)),
        _ => new SolidColorBrush(Color.FromRgb(198, 209, 219))
    };

    public string ActualSpeedDisplay => $"{_actualSpeed:N1} m/min";

    public double SpeedPercent => ToPercent(_actualSpeed, Math.Max(ReadDecimalOrDefault(SpeedSetpointText, 1), 1));

    public string ProgressDisplay => $"{ProgressPercent:N0}%";

    public double ProgressPercent => ToPercent(_progressMeters, Math.Max(ReadDecimalOrDefault(TargetLengthText, 1), 1));

    public string TensionDisplay => $"{_tensionKn:N1} kN";

    public double TensionPercent => ToPercent(_tensionKn, 18);

    public string ScrapDisplay => $"{_scrapMm:N1} mm";

    public string MesTransferStatus
    {
        get => _mesTransferStatus;
        private set => SetField(ref _mesTransferStatus, value);
    }

    public string PlcPayload
    {
        get => _plcPayload;
        private set => SetField(ref _plcPayload, value);
    }

    private void LoadRecipeButton_Click(object sender, RoutedEventArgs e)
    {
        LoadRecipe();
    }

    private void StartButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            LoadRecipe();
            _runState = MachineRunState.Running;
            _machineTimer.Start();
            AddMessage("Comando", "Ciclo di taglio avviato dall'operatore.");
            RefreshMachineState();
        }
        catch (Exception ex)
        {
            AddMessage("Allarme", ex.Message);
            _runState = MachineRunState.Alarm;
            RefreshMachineState();
        }
    }

    private void PauseButton_Click(object sender, RoutedEventArgs e)
    {
        if (_runState == MachineRunState.Running)
        {
            _runState = MachineRunState.Paused;
            _machineTimer.Stop();
            _actualSpeed = 0;
            AddMessage("Comando", "Ciclo messo in pausa.");
            RefreshRuntimeValues();
            RefreshMachineState();
        }
    }

    private void StopButton_Click(object sender, RoutedEventArgs e)
    {
        _runState = MachineRunState.Idle;
        _machineTimer.Stop();
        _actualSpeed = 0;
        AddMessage("Comando", "Ciclo arrestato.");
        RefreshRuntimeValues();
        RefreshMachineState();
    }

    private void MachineTimer_Tick(object? sender, EventArgs e)
    {
        decimal setpoint = ReadDecimalOrDefault(SpeedSetpointText, 0);
        decimal targetLength = Math.Max(ReadDecimalOrDefault(TargetLengthText, 1), 1);

        _actualSpeed = Approach(_actualSpeed, setpoint, 3.5m);
        _progressMeters += _actualSpeed / 120m;
        _tensionKn = 8.5m + (decimal)(Math.Sin(DateTime.Now.Second / 4.0) * 1.4);

        if (_progressMeters >= targetLength)
        {
            _progressMeters = targetLength;
            _runState = MachineRunState.Idle;
            _machineTimer.Stop();
            _actualSpeed = 0;
            MesTransferStatus = SendToMes ? "Consuntivo produzione inviato al MES" : "Invio MES disabilitato";
            PublishProductionCompleted();
            AddMessage("Fine ciclo", $"Completato ordine {WorkOrder} per bobina {CoilCode}.");
            RefreshMachineState();
        }

        RefreshRuntimeValues();
    }

    private void LoadRecipe()
    {
        decimal coilWidth = ReadDecimal(CoilWidthText);
        decimal stripWidth = ReadDecimal(StripWidthText);
        int activeBlades = ReadInt(ActiveBladesText);
        decimal speedSetpoint = ReadDecimal(SpeedSetpointText);
        decimal targetLength = ReadDecimal(TargetLengthText);

        if (coilWidth <= 0 || stripWidth <= 0 || activeBlades <= 0 || speedSetpoint <= 0 || targetLength <= 0)
        {
            throw new InvalidOperationException("La ricetta contiene valori non validi.");
        }

        decimal usedWidth = activeBlades * stripWidth;
        if (usedWidth > coilWidth)
        {
            throw new InvalidOperationException("La somma dei nastri supera la larghezza della bobina madre.");
        }

        _scrapMm = coilWidth - usedWidth;
        _progressMeters = 0;
        _actualSpeed = 0;
        _tensionKn = 8.5m;
        MesTransferStatus = SendToMes ? "MES abilitato" : "MES disabilitato";

        BuildBladeLayout(activeBlades);

        var recipe = new PlcRecipePayload(
            WorkOrder,
            CoilCode,
            coilWidth,
            stripWidth,
            activeBlades,
            speedSetpoint,
            targetLength,
            _scrapMm,
            DateTime.Now);

        PlcPayload = _machineControlService.SendRecipeToPlc(recipe);
        PublishPlcRecipe(recipe);
        AddMessage("Ricetta", $"Ricetta caricata: {activeBlades} lame, nastro {stripWidth:N1} mm, velocità {speedSetpoint:N1} m/min.");
        RefreshRuntimeValues();
    }

    private void PublishPlcRecipe(PlcRecipePayload recipe)
    {
        _integrationBus.Publish(
            IntegrationMessageTypes.PlcRecipeLoaded,
            "CoilCutOperatorHmiWpf",
            new PlcRecipeMessage(
                recipe.WorkOrder,
                recipe.CoilCode,
                recipe.CoilWidthMm,
                recipe.StripWidthMm,
                recipe.ActiveBlades,
                recipe.SpeedSetpointMMin,
                recipe.TargetLengthM,
                recipe.ScrapMm));
    }

    private void PublishProductionCompleted()
    {
        _integrationBus.Publish(
            IntegrationMessageTypes.ProductionCompleted,
            "CoilCutOperatorHmiWpf",
            new ProductionCompletedMessage(
                WorkOrder,
                CoilCode,
                _progressMeters,
                _scrapMm));
    }

    private void BuildBladeLayout(int activeBlades)
    {
        BladeLayoutColumns = activeBlades;
        BladePositions.Clear();

        for (int i = 0; i < activeBlades; i++)
        {
            BladePositions.Add(new BladePosition($"L{i + 1}"));
        }
    }

    private void AddMessage(string level, string message)
    {
        MachineMessages.Insert(0, new MachineMessage(
            DateTime.Now.ToString("HH:mm:ss", CultureInfo.CurrentCulture),
            level,
            message));

        while (MachineMessages.Count > 12)
        {
            MachineMessages.RemoveAt(MachineMessages.Count - 1);
        }
    }

    private void RefreshMachineState()
    {
        OnPropertyChanged(nameof(MachineState));
        OnPropertyChanged(nameof(MachineStateBrush));
        OnPropertyChanged(nameof(MachineStateBackground));
        OnPropertyChanged(nameof(MachineStateBorder));
    }

    private void RefreshRuntimeValues()
    {
        OnPropertyChanged(nameof(ActualSpeedDisplay));
        OnPropertyChanged(nameof(SpeedPercent));
        OnPropertyChanged(nameof(ProgressDisplay));
        OnPropertyChanged(nameof(ProgressPercent));
        OnPropertyChanged(nameof(TensionDisplay));
        OnPropertyChanged(nameof(TensionPercent));
        OnPropertyChanged(nameof(ScrapDisplay));
    }

    private static decimal Approach(decimal value, decimal target, decimal step)
    {
        if (value < target)
        {
            return Math.Min(value + step, target);
        }

        return Math.Max(value - step, target);
    }

    private static double ToPercent(decimal value, decimal max)
    {
        return (double)Math.Clamp(value / max * 100, 0, 100);
    }

    private static decimal ReadDecimal(string value)
    {
        if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.CurrentCulture, out decimal result))
        {
            return result;
        }

        if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out result))
        {
            return result;
        }

        throw new FormatException($"Valore numerico non valido: {value}");
    }

    private static decimal ReadDecimalOrDefault(string value, decimal fallback)
    {
        try
        {
            return ReadDecimal(value);
        }
        catch
        {
            return fallback;
        }
    }

    private static int ReadInt(string value)
    {
        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.CurrentCulture, out int result))
        {
            return result;
        }

        throw new FormatException($"Valore intero non valido: {value}");
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public enum MachineRunState
{
    Idle,
    Running,
    Paused,
    Alarm
}

public sealed record MachineMessage(string Time, string Level, string Message);

public sealed record BladePosition(string Label);

public sealed record PlcRecipePayload(
    string WorkOrder,
    string CoilCode,
    decimal CoilWidthMm,
    decimal StripWidthMm,
    int ActiveBlades,
    decimal SpeedSetpointMMin,
    decimal TargetLengthM,
    decimal ScrapMm,
    DateTime Timestamp);

public sealed class MachineControlService
{
    public string SendRecipeToPlc(PlcRecipePayload recipe)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        return JsonSerializer.Serialize(new
        {
            Command = "LOAD_RECIPE",
            Target = "PLC_COIL_SLITTER_01",
            Recipe = recipe
        }, options);
    }
}
