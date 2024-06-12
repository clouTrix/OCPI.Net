using System.Text.Json.Serialization;

namespace OCPI.Contracts;

public class OcpiSession
{
    ///
    /// OCPI 
    ///

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("kwh")]
    public decimal? Kwh { get; set; }

    [JsonPropertyName("auth_method")]
    public AuthMethodType? AuthMethod { get; set; }

    [JsonPropertyName("meter_id")]
    public string? MeterId { get; set; }

    [JsonPropertyName("currency")]
    public CurrencyCode? Currency { get; set; }

    [JsonPropertyName("charging_periods")]
    public IEnumerable<OcpiChargingPeriod>? ChargingPeriods { get; set; }

    //FIXME
    [JsonPropertyName("total_cost")]
    public OcpiPrice? TotalCost { get; set; }

    [JsonPropertyName("status")]
    public SessionStatus? Status { get; set; }

    [JsonPropertyName("last_updated")]
    public DateTime? LastUpdated { get; set; }

    ///
    /// OCPI 2.2.1 
    ///
    [OcpiIntroduced("2.2.1")]
    [JsonPropertyName("country_code")]
    public CountryCode? CountryCode { get; set; }

    [OcpiIntroduced("2.2.1")]
    [JsonPropertyName("party_id")]
    public string? PartyId { get; set; }

    [OcpiIntroduced("2.2.1")]
    [JsonPropertyName("start_date_time")]
    public DateTime? StartDateTime { get; set; }

    [OcpiIntroduced("2.2.1")]
    [JsonPropertyName("end_date_time")]
    public DateTime? EndDateTime { get; set; }

    [OcpiIntroduced("2.2.1")]
    [JsonPropertyName("cdr_token")]
    public OcpiCdrToken? CdrToken { get; set; }

    [OcpiIntroduced("2.2.1")]
    [JsonPropertyName("authorization_reference")]
    public string? AuthorizationReference { get; set; }

    [OcpiIntroduced("2.2.1")]
    [JsonPropertyName("location_id")]
    public string? LocationId { get; set; }

    [OcpiIntroduced("2.2.1")]
    [JsonPropertyName("evse_uid")]
    public string? EvseId { get; set; }

    [OcpiIntroduced("2.2.1")]
    [JsonPropertyName("connector_id")]
    public string? ConnectorId { get; set; }
    
    ///
    /// OCPI 2.1.1
    ///
    [OcpiDeprecated("2.1.1")]
    [JsonPropertyName("location")]
    public OcpiLocation? Location { get; set; }

    [OcpiDeprecated("2.1.1")]
    [JsonPropertyName("start_datetime")]
    public DateTime? StartDateTime211 {
        get => this.StartDateTime;
        set => this.StartDateTime = value;
    }

    [OcpiDeprecated("2.1.1")]
    [JsonPropertyName("end_datetime")]
    public DateTime? EndDateTime211 {
        get => this.EndDateTime;
        set => this.EndDateTime = value;
    }

    [OcpiDeprecated("2.1.1")]
    [JsonPropertyName("auth_id")]
    public string? AuthId { get; set; }

    // [OcpiDeprecated("2.1.1")]
    // [JsonPropertyName("total_cost")]
    // public Decimal? TotalCost211 {
    //     get => this.TotalCost?.ExclVat;
    //     set => 
    //         this.TotalCost = new OcpiPrice { ExclVat = value };
    // }
}
