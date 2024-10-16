using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using FluentAssertions;
using OCPI.Serdes.Json;
using Xunit.Abstractions;

namespace OCPI.Tests.JsonConverters.Serializing;

public class Poco
{
    [JsonPropertyName("id")]
    [OcpiDeprecated("2.1.1")]
    public int? IdV0 { get; set; }
    
    [JsonPropertyName("id")]
    [OcpiIntroduced("2.2.1")]
    public string? IdV1 { get; set; }
}

public class ForPoco(ITestOutputHelper output) {
    private static readonly Poco poco = 
        new Poco {
            IdV0 = 666,
            IdV1 = "666***666"
        };
    
    [Fact]
    public void Poco_Can_Be_Serialized_ForVersion211() {
        var options = new JsonSerializerOptions();
        ConfigureOcpiJsonExtension.ConfigureJsonSerdes(options);
        options.Converters.Add(new OcpiJsonConverterExtraSettings(() => [ OcpiVersion.v2_1_1 ]));
        options.Converters.Add(new OcpiJsonConverter<Poco>());

        var json = JsonNode.Parse(JsonSerializer.Serialize(poco, options));

        json.Should().NotBeNull();
        output.WriteLine(
            json!.ToJsonString(new JsonSerializerOptions { WriteIndented = true })
        );
    }
    
    [Fact]
    public void Poco_Can_Be_Serialized_ForVersion221() {
        var options = new JsonSerializerOptions();
        ConfigureOcpiJsonExtension.ConfigureJsonSerdes(options);
        options.Converters.Add(new OcpiJsonConverterExtraSettings(() => [ OcpiVersion.v2_2_1 ]));
        options.Converters.Add(new OcpiJsonConverter<Poco>());

        var json = JsonNode.Parse(JsonSerializer.Serialize(poco, options));

        json.Should().NotBeNull();
        output.WriteLine(
            json!.ToJsonString(new JsonSerializerOptions { WriteIndented = true })
        );
    }
}