using System.Text.Json.Serialization;

namespace OCPI.Contracts;

public class OcpiCredentials
{
    [JsonPropertyName("token")]
    public string? Token { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [OcpiDeprecated("2.1.1")]
    [JsonPropertyName("party_id")]
    public string? PartyId { get; set; }

    [OcpiDeprecated("2.1.1")]
    [JsonPropertyName("country_code")]
    public string? CountryCode { get; set; }
    
    [OcpiDeprecated("2.1.1")]
    [JsonPropertyName("business_details")]
    public OcpiBusinessDetails? BusinessDetails { get; set; }
    
    [OcpiIntroduced("2.2.1")]
    [JsonPropertyName("roles")]
    public IEnumerable<OcpiCredentialsRole>? Roles { get; set; }
}
