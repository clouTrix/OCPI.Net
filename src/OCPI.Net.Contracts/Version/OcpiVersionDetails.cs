using System.Text.Json.Serialization;

namespace OCPI.Contracts;

public class OcpiVersionDetails
{
    [JsonPropertyName("version")]
    public OcpiVersion? Version { get; set; }

    [JsonPropertyName("endpoints")]
    public IEnumerable<OcpiEndpoint>? Endpoints { get; set; }
}
