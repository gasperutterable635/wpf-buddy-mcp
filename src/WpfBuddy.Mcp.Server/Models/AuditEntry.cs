using System.Text.Json.Serialization;

namespace WpfBuddy.Mcp.Server.Models;

public sealed class AuditEntry
{
    [JsonPropertyName("timestampUtc")]
    public DateTime TimestampUtc { get; set; }

    [JsonPropertyName("tool")]
    public string Tool { get; set; } = string.Empty;

    [JsonPropertyName("selector")]
    public ElementCriteria? Selector { get; set; }

    [JsonPropertyName("parameters")]
    public Dictionary<string, object?>? Parameters { get; set; }

    [JsonPropertyName("result")]
    public string Result { get; set; } = "success";

    [JsonPropertyName("error")]
    public string? Error { get; set; }
}
