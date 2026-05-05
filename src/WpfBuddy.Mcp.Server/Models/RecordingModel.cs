using System.Text.Json.Serialization;

namespace WpfBuddy.Mcp.Server.Models;

public sealed class RecordingModel
{
    [JsonPropertyName("schemaVersion")]
    public string SchemaVersion { get; set; } = "0.1";

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("app")]
    public RecordingAppInfo? App { get; set; }

    [JsonPropertyName("policy")]
    public RecordingPolicy? Policy { get; set; }

    [JsonPropertyName("steps")]
    public List<RecordingStep> Steps { get; set; } = [];
}

public sealed class RecordingAppInfo
{
    [JsonPropertyName("process")]
    public string? Process { get; set; }

    [JsonPropertyName("executablePath")]
    public string? ExecutablePath { get; set; }
}

public sealed class RecordingPolicy
{
    [JsonPropertyName("allowCoordinateFallback")]
    public bool AllowCoordinateFallback { get; set; }

    [JsonPropertyName("allowDestructive")]
    public bool AllowDestructive { get; set; } = true;

    [JsonPropertyName("redactValues")]
    public bool RedactValues { get; set; } = true;

    [JsonPropertyName("timeoutMs")]
    public int TimeoutMs { get; set; } = 30000;

    [JsonPropertyName("maxRetries")]
    public int MaxRetries { get; set; } = 3;
}

public sealed class RecordingStep
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("action")]
    public string? Action { get; set; }

    [JsonPropertyName("assert")]
    public string? Assert { get; set; }

    [JsonPropertyName("selector")]
    public ElementCriteria? Selector { get; set; }

    [JsonPropertyName("value")]
    public string? Value { get; set; }

    [JsonPropertyName("timestampUtc")]
    public DateTime TimestampUtc { get; set; }
}

public sealed class AppInfo
{
    [JsonPropertyName("processId")]
    public int ProcessId { get; set; }

    [JsonPropertyName("processName")]
    public string ProcessName { get; set; } = string.Empty;

    [JsonPropertyName("mainWindowTitle")]
    public string? MainWindowTitle { get; set; }

    [JsonPropertyName("executablePath")]
    public string? ExecutablePath { get; set; }
}
