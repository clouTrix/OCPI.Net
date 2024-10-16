using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;

namespace OCPI.Tests.JsonConverters.Deserializing;

public class SomeModel
{
    public int? Id { get; set; }
    
    [JsonPropertyName("code"), JsonIgnore]
    public string? CodeV0 { get; set; }
    
    [JsonPropertyName("code")]
    public int? CodeV1 { get; set; }
}

public class ForDefaults
{
    [Fact]
    public void Test()
    {
        var json = """
                   { "id"  : 666,
                     "code": 777
                   }
                   """;
        
        var obj = JsonSerializer.Deserialize<SomeModel>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        });
        obj.Should().NotBeNull();
        obj!.Id.Should().Be(666);
        obj!.CodeV0.Should().BeNull();
        obj!.CodeV1.Should().Be(777);
    }
}