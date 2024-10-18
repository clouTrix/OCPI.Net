using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using OCPI.Serdes.Json;
using Xunit.Abstractions;
using Xunit.Extensions.Logging;

namespace OCPI.Tests.JsonConverters.Deserializing;

public class ForPoco(ITestOutputHelper output)
{
    private static readonly string pocoJson_211 = """
                                                  {
                                                    "id": 666,
                                                    "unknown1": "beelzebub",
                                                    "unknown2": 777
                                                  } 
                                                  """;

    private static readonly string pocoJson_221 = """
                                                  {
                                                    "id": "666***666",
                                                    "unknown1": "beelzebub",
                                                    "unknown2": 777
                                                  } 
                                                  """;

    private ILoggerFactory FactoryFor(LogLevel rootLevel)
        => LoggerFactory.Create(
            builder => builder.AddProvider(new XunitLoggerProvider(output, (_,level) => level >= rootLevel))  
                .SetMinimumLevel(LogLevel.Trace)
        );

    [Fact]
    public void Poco_Can_HandleUnmappedProperties() {
        var options = new JsonSerializerOptions();
        ConfigureOcpiJsonExtension.ConfigureJsonSerdes(options);
        options.Converters.Add(new OcpiJsonConverterExtraSettings(() => [ OcpiVersion.v2_1_1 ]));
        options.Converters.Add(new OcpiJsonConverter<Poco>(FactoryFor(LogLevel.Debug).CreateLogger<OcpiJsonConverter<Poco>>()));

        var poco = JsonSerializer.Deserialize<Poco>(pocoJson_211, options);
        poco.Should().NotBeNull();
        poco!._UnknownFields.Should().NotBeNull();
        poco!._UnknownFields!.Should().NotBeEmpty();
        
        poco!._UnknownFields!["unknown1"].GetValue<string>().Should().Be("beelzebub");
        poco!._UnknownFields!["unknown2"].GetValue<int>().Should().Be(777);
    }

    [Fact]
    public void Poco_Can_Log() {
        var options = new JsonSerializerOptions();
        ConfigureOcpiJsonExtension.ConfigureJsonSerdes(options);
        options.Converters.Add(new OcpiJsonConverterExtraSettings(() => [ OcpiVersion.v2_1_1, OcpiVersion.v2_2_1 ]));
        options.Converters.Add(new OcpiJsonConverter<Poco>(FactoryFor(LogLevel.Debug).CreateLogger<OcpiJsonConverter<Poco>>()));

        _ = JsonSerializer.Deserialize<Poco>(pocoJson_221, options);
        //TODO: check logs?
    }
    
    [Fact]
    public void Poco_Can_Be_Deserialized_ForVersion211() {
        var options = new JsonSerializerOptions();
        ConfigureOcpiJsonExtension.ConfigureJsonSerdes(options);
        options.Converters.Add(new OcpiJsonConverterExtraSettings(() => [ OcpiVersion.v2_1_1 ]));
        options.Converters.Add(new OcpiJsonConverter<Poco>());

        var poco = JsonSerializer.Deserialize<Poco>(pocoJson_211, options);
        poco.Should().NotBeNull();
        poco!.IdV0.Should().Be(666);
        poco!.IdV1.Should().BeNull();
    }
    
    [Fact]
    public void Poco_Can_Be_Deserialized_ForVersion221() {
        var options = new JsonSerializerOptions();
        ConfigureOcpiJsonExtension.ConfigureJsonSerdes(options);
        options.Converters.Add(new OcpiJsonConverterExtraSettings(() => [ OcpiVersion.v2_2_1 ]));
        options.Converters.Add(new OcpiJsonConverter<Poco>());

        var poco = JsonSerializer.Deserialize<Poco>(pocoJson_221, options);
        poco.Should().NotBeNull();
        poco!.IdV0.Should().BeNull();
        poco!.IdV1.Should().Be("666***666");
    }
}