﻿using System.Text.Json.Serialization;

namespace OCPI.Contracts;

public class OcpiLocationReferences
{
    /// <summary>
    /// Unique identifier for the location.
    /// </summary>
    [JsonPropertyName("location_id")]
    public string? LocationId { get; set; }

    /// <summary>
    /// Unique identifiers for EVSEs within the CPO’s platform for the EVSE within the
    /// given location.
    /// </summary>
    [JsonPropertyName("evse_uids")]
    public string? EvseUids { get; set; }

    ///
    /// OCPI 2.1.1 
    ///

    /// <summary>
    /// Identifies the connectors within the given EVSEs.
    /// </summary>
    [OcpiDeprecated(after: "2.1.1")]
    [JsonPropertyName("connector_ids")]
    public string? ConnectorIds { get; set; }
}

