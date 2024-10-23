using System.Text.Json;
using System.Text.Json.Nodes;
using FluentAssertions;
using OCPI.Serdes.Json;
using Xunit.Abstractions;

namespace OCPI.Tests.JsonConverters.Serializing;

public class ForPoco(/*ITestOutputHelper output*/) {
    private static readonly Poco poco = 
        new Poco {
            IdV0 = 666,
            IdV1 = "666***666",
            _UnknownFields = new Dictionary<string, JsonNode>
            {
                { "unknown1", JsonValue.Create("beelzebub") },
                { "unknown2", JsonValue.Create(666)         }
            }
        };
    
    [Fact]
    public void Poco_Can_Be_Serialized_ForVersion211() {
        var options = new JsonSerializerOptions();
        ConfigureOcpiJsonExtension.ConfigureJsonSerdes(options);
        options.Converters.Add(new OcpiJsonConverterExtraSettings(() => [ OcpiVersion.v2_1_1 ]));
        options.Converters.Add(new OcpiJsonConverter<Poco>());

        var json = JsonNode.Parse(JsonSerializer.Serialize(poco, options))?.AsObject();
        // output.WriteLine(
        //     json?.ToJsonString(new JsonSerializerOptions { WriteIndented = true })
        // );

        json.Should().NotBeNull();
        json!["id"]?.GetValue<int>().Should().Be(666);

        // _UnknownFields are NOT serialized
        json!.ToDictionary().Select(kv => kv.Key).Should().Equal(["id"]);
    }
    
    [Fact]
    public void Poco_Can_Be_Serialized_ForVersion221() {
        var options = new JsonSerializerOptions();
        ConfigureOcpiJsonExtension.ConfigureJsonSerdes(options);
        options.Converters.Add(new OcpiJsonConverterExtraSettings(() => [ OcpiVersion.v2_2_1 ]));
        options.Converters.Add(new OcpiJsonConverter<Poco>());

        var json = JsonNode.Parse(JsonSerializer.Serialize(poco, options))?.AsObject();
        // output.WriteLine(
        //     json?.ToJsonString(new JsonSerializerOptions { WriteIndented = true })
        // );

        json.Should().NotBeNull();
        json!["id"]?.GetValue<string>().Should().Be("666***666");
        
        // _UnknownFields are NOT serialized
        json!.ToDictionary().Select(kv => kv.Key).Should().Equal(["id"]);

    }
}