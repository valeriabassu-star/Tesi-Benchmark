using System.Text.Json;

namespace CoilCut.Shared;

public sealed class JsonIntegrationBus
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };

    private readonly string _messageFolder;

    public JsonIntegrationBus()
        : this(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "CoilCutIntegratedSystem",
            "messages"))
    {
    }

    public JsonIntegrationBus(string messageFolder)
    {
        _messageFolder = messageFolder;
        Directory.CreateDirectory(_messageFolder);
    }

    public string MessageFolder => _messageFolder;

    public void Publish<TPayload>(string messageType, string source, TPayload payload)
    {
        var envelope = new IntegrationEnvelope<TPayload>(
            messageType,
            source,
            DateTime.Now,
            payload);

        string safeType = MakeSafeFileName(messageType);
        string latestPath = Path.Combine(_messageFolder, $"{safeType}.latest.json");
        string archivePath = Path.Combine(_messageFolder, $"{safeType}.{DateTime.Now:yyyyMMdd_HHmmss_fff}.json");
        string json = JsonSerializer.Serialize(envelope, SerializerOptions);

        File.WriteAllText(latestPath, json);
        File.WriteAllText(archivePath, json);
    }

    public IntegrationEnvelope<JsonElement>? ReadLatest(string messageType)
    {
        string path = Path.Combine(_messageFolder, $"{MakeSafeFileName(messageType)}.latest.json");
        if (!File.Exists(path))
        {
            return null;
        }

        string json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<IntegrationEnvelope<JsonElement>>(json, SerializerOptions);
    }

    private static string MakeSafeFileName(string value)
    {
        foreach (char invalidChar in Path.GetInvalidFileNameChars())
        {
            value = value.Replace(invalidChar, '_');
        }

        return value.Trim().Replace(' ', '_').ToLowerInvariant();
    }
}

public sealed record IntegrationEnvelope<TPayload>(
    string MessageType,
    string Source,
    DateTime Timestamp,
    TPayload Payload);
