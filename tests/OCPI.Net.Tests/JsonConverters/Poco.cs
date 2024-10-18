using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace OCPI.Tests.JsonConverters;

public class Poco
{
    [JsonPropertyName("id")]
    [OcpiDeprecated("2.1.1")]
    public int? IdV0 { get; set; }
    
    [JsonPropertyName("id")]
    [OcpiIntroduced("2.2.1")]
    public string? IdV1 { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonNode>? _UnknownFields { get; set; }
}