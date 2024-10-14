using System.Text.Json.Serialization;

namespace OCPI.Contracts;

public class OcpiVersionInfo
{
    [JsonPropertyName("version")]
    public OcpiVersion? Version { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }
}
