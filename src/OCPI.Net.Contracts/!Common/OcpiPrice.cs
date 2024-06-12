using System.Text.Json;
using System.Text.Json.Serialization;

namespace OCPI.Contracts;

//little hack to get the custom JsonTypeConverter working..
public class OcpiPriceBase
{
    [JsonPropertyName("excl_vat")]
    public decimal? ExclVat { get; set; }

    [JsonPropertyName("incl_vat")]
    public decimal? InclVat { get; set; }
}

[JsonConverter(typeof(OcpiPriceConverter))]
public class OcpiPrice : OcpiPriceBase { }

/// <summary>
/// 
/// </summary>
public class OcpiPriceConverter: JsonConverter<OcpiPrice> {
    public override OcpiPrice? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.TokenType switch {
            JsonTokenType.Number => new OcpiPrice { ExclVat = reader.GetDecimal() },
                               _ => null
        };
    
    public override void Write(Utf8JsonWriter writer, OcpiPrice value, JsonSerializerOptions options) {
        //TODO: fix infinite recursion !!!
        JsonSerializer.Serialize(writer, (OcpiPriceBase)value, options);
    }
}
