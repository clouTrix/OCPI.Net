﻿using System.Text.Json.Serialization;

namespace OCPI.Contracts;

public class OcpiCdrToken
{
    [JsonPropertyName("uid")]
    public string? Uid { get; set; }

    [JsonPropertyName("type")]
    public TokenType? Type { get; set; }

    [JsonPropertyName("contract_id")]
    public string? ContractId { get; set; }
    
    ///
    /// OCPI 2.2.1 
    ///
    [OcpiIntroduced("2.2.1")]
    [JsonPropertyName("country_code")]
    public CountryCode? CountryCode { get; set; }
    
    [OcpiIntroduced("2.2.1")]
    [JsonPropertyName("party_id")]
    public string? PartyId { get; set; }
}
