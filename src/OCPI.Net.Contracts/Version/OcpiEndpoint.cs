using System.Text.Json.Serialization;

namespace OCPI.Contracts;

public class OcpiEndpoint
{
    [JsonPropertyName("identifier")]
    public OcpiModule? Identifier { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }
    
    ///
    /// OCPI 2.2.1 
    ///
    [OcpiIntroduced("2.2.1")]
    [JsonPropertyName("role")]
    public InterfaceRole? Role { get; set; }
}
