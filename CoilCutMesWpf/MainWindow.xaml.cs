using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows;
using CoilCut.Shared;

namespace CoilCutMesWpf;

public partial class MainWindow : Window, INotifyPropertyChanged
{
    private readonly JsonIntegrationBus _integrationBus = new();
    private readonly MesIntegrationService _mesIntegrationService = new();

    private string _orderCode = "ORD-2026-014";
    private string _coilId = "COIL-A42";
    private string _motherCoilWidthText = "1250";
    private string _stripWidthText = "185";
    private string _requestedStripsText = "6";
    private string _coilLengthText = "850";
    private int _availableStrips;
    private decimal _scrapWidth;
    private decimal _efficiency;
    private string _mesStatus = "MES simulato disponibile";
    private string _lastMesPayload = "Premere \"Invia al MES\" dopo aver calcolato il piano di taglio.";

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
        CalculateCutPlan();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<CutPlanRow> CutPlanRows { get; } = new();

    public ObservableCollection<ProductionEvent> ProductionEvents { get; } = new();

    public string OrderCode
    {
        get => _orderCode;
        set => SetField(ref _orderCode, value);
    }

    public string CoilId
    {
        get => _coilId;
        set => SetField(ref _coilId, value);
    }

    public string MotherCoilWidthText
    {
        get => _motherCoilWidthText;
        set => SetField(ref _motherCoilWidthText, value);
    }

    public string StripWidthText
    {
        get => _stripWidthText;
        set => SetField(ref _stripWidthText, value);
    }

    public string RequestedStripsText
    {
        get => _requestedStripsText;
        set => SetField(ref _requestedStripsText, value);
    }

    public string CoilLengthText
    {
        get => _coilLengthText;
        set => SetField(ref _coilLengthText, value);
    }

    public int AvailableStrips
    {
        get => _availableStrips;
        private set => SetField(ref _availableStrips, value);
    }

    public string ScrapWidthDisplay => $"{_scrapWidth:N1} mm";

    public string EfficiencyDisplay => $"{_efficiency:P1}";

    public string MesStatus
    {
        get => _mesStatus;
        private set => SetField(ref _mesStatus, value);
    }

    public string LastMesPayload
    {
        get => _lastMesPayload;
        private set => SetField(ref _lastMesPayload, value);
    }

    private void CalculateButton_Click(object sender, RoutedEventArgs e)
    {
        CalculateCutPlan();
    }

    private async void SendMesButton_Click(object sender, RoutedEventArgs e)
    {
        if (CutPlanRows.Count == 0)
        {
            MessageBox.Show("Calcolare prima il piano di taglio.", "MES", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        MesStatus = "Invio dati in corso...";

        var payload = new MesProductionPayload(
            OrderCode,
            CoilId,
            ReadDecimal(MotherCoilWidthText),
            ReadDecimal(StripWidthText),
            AvailableStrips,
            _scrapWidth,
            Math.Round(_efficiency * 100, 2),
            DateTime.Now,
            CutPlanRows.ToList());

        LastMesPayload = await _mesIntegrationService.SendProductionDataAsync(payload);
        PublishMesCutPlan(payload);
        MesStatus = "Ultimo invio MES completato";

        AddEvent("Dati inviati al MES",
            $"Ordine {OrderCode}: {AvailableStrips} nastri, scarto {_scrapWidth:N1} mm, efficienza {_efficiency:P1}.");
    }

    private void PublishMesCutPlan(MesProductionPayload payload)
    {
        _integrationBus.Publish(
            IntegrationMessageTypes.MesCutPlanSent,
            "CoilCutMesWpf",
            new MesCutPlanMessage(
                payload.OrderCode,
                payload.CoilId,
                payload.MotherCoilWidthMm,
                payload.StripWidthMm,
                payload.ProducedStrips,
                payload.ScrapWidthMm,
                payload.EfficiencyPercent));
    }

    private void CalculateCutPlan()
    {
        try
        {
            decimal motherCoilWidth = ReadDecimal(MotherCoilWidthText);
            decimal stripWidth = ReadDecimal(StripWidthText);
            int requestedStrips = ReadInt(RequestedStripsText);
            decimal coilLength = ReadDecimal(CoilLengthText);

            if (motherCoilWidth <= 0 || stripWidth <= 0 || requestedStrips <= 0 || coilLength <= 0)
            {
                throw new InvalidOperationException("Tutti i parametri devono essere maggiori di zero.");
            }

            int maxStrips = (int)Math.Floor(motherCoilWidth / stripWidth);
            AvailableStrips = Math.Min(maxStrips, requestedStrips);
            decimal usedWidth = AvailableStrips * stripWidth;

            _scrapWidth = Math.Max(0, motherCoilWidth - usedWidth);
            _efficiency = motherCoilWidth == 0 ? 0 : usedWidth / motherCoilWidth;

            OnPropertyChanged(nameof(ScrapWidthDisplay));
            OnPropertyChanged(nameof(EfficiencyDisplay));

            CutPlanRows.Clear();
            for (int i = 1; i <= AvailableStrips; i++)
            {
                CutPlanRows.Add(new CutPlanRow(
                    i,
                    $"{CoilId}-S{i:00}",
                    stripWidth,
                    coilLength,
                    i % 2 == 0 ? "Magazzino semilavorati" : "Linea assemblaggio",
                    "Pianificato"));
            }

            AddEvent("Piano di taglio calcolato",
                $"Bobina {CoilId}: larghezza {motherCoilWidth:N1} mm, nastro {stripWidth:N1} mm, scarto {_scrapWidth:N1} mm.");
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Parametri non validi", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void AddEvent(string title, string detail)
    {
        ProductionEvents.Insert(0, new ProductionEvent(
            DateTime.Now.ToString("HH:mm:ss", CultureInfo.CurrentCulture) + " - " + title,
            detail));
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

public sealed record CutPlanRow(
    int Position,
    string StripId,
    decimal WidthMm,
    decimal LengthM,
    string Destination,
    string Status);

public sealed record ProductionEvent(string Title, string Detail);

public sealed record MesProductionPayload(
    string OrderCode,
    string CoilId,
    decimal MotherCoilWidthMm,
    decimal StripWidthMm,
    int ProducedStrips,
    decimal ScrapWidthMm,
    decimal EfficiencyPercent,
    DateTime Timestamp,
    IReadOnlyCollection<CutPlanRow> CutPlan);

public sealed class MesIntegrationService
{
    public async Task<string> SendProductionDataAsync(MesProductionPayload payload)
    {
        await Task.Delay(600);

        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        return JsonSerializer.Serialize(payload, options);
    }
}
