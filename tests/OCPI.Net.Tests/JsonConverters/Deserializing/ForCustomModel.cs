using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using OCPI.Serdes.Json;
using Xunit.Abstractions;
using Xunit.Extensions.Logging;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace OCPI.Tests.JsonConverters.Deserializing;

public class MyModel
{
    [JsonPropertyName("@name")]
    public string? Name { get; set; }

    [JsonPropertyName("@description")]
    public string? Description { get; set; }

    [OcpiIntroduced("2.2.1")]
    [JsonPropertyName("code")]
    public string? CodeV1 { get; set; }
    
    [OcpiDeprecated("2.1.1")]
    [JsonPropertyName("code")]
    public int? CodeV0 { get; set; }
    
    public string? AlwaysPresent { get; set; }

    [System.Text.Json.Serialization.JsonIgnore]
    public string? ShouldIgnore { get; set; }
}

public class ForCustomModel(ITestOutputHelper output)
{
    private ILoggerFactory LogFactory(LogLevel rootLevel)
        => LoggerFactory.Create(
            builder => builder.AddProvider(new XunitLoggerProvider(output, (_,level) => level >= rootLevel))  
                .SetMinimumLevel(LogLevel.Trace)
        );
    
    JsonSerializerOptions Options(OcpiVersion? versionMaybe = null)
    {
        var opts = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy   = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters =
                {
                    new OcpiJsonConverter<MyModel>(LogFactory(LogLevel.Debug).CreateLogger<OcpiJsonConverter<MyModel>>())
                }
        };
        if(versionMaybe.HasValue) opts.Converters.Add(new OcpiJsonConverterExtraSettings(() => [ versionMaybe.Value ]));
        return opts;
    }
    
    [Fact]
    public void Deserializing_Can_JsonIgnoreProperties()
    {
      var json = """
                 { "@name"        : "my-name",
                   "@description" : "my-description",
                   "code"         : 666,
                   "should_ignore"  : "*************",
                   "always_present" : "other"
                 }
                 """;

      var obj = JsonSerializer.Deserialize<MyModel>(json, Options());
      obj.Should().NotBeNull();
      obj!.ShouldIgnore.Should().BeNull();
      obj!.AlwaysPresent.Should().NotBeNull();
    }
    
    [Fact]
    public void Deserializing_Can_HandleDeprecatedVersions()
    {
        var json = """
                   { "@name"        : "my-name",
                     "@description" : "my-description",
                     "code"         : 666,
                     "should_ignore"  : "*************",
                     "always_present" : "other"
                   }
                   """;

        var obj = JsonSerializer.Deserialize<MyModel>(json, Options(OcpiVersion.v2_1_1));
        obj.Should().NotBeNull();
        obj!.CodeV0.Should().Be(666);
        obj!.CodeV1.Should().BeNull();
    }
    
    [Fact]
    public void Deserializing_Can_HandleIntroducedVersions()
    {
        var json = """
                   { "@name"        : "my-name",
                     "@description" : "my-description",
                     "code"         : "666***666",
                     "should_ignore"  : "*************",
                     "always_present" : "other"
                   }
                   """;

        var obj = JsonSerializer.Deserialize<MyModel>(json, Options(OcpiVersion.v2_2_1));
        obj.Should().NotBeNull();
        obj!.CodeV0.Should().BeNull();
        obj!.CodeV1.Should().Be("666***666");
    }

}