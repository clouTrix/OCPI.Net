using System.Text.Json;
using System.Text.Json.Nodes;
using FluentAssertions;
using OCPI.Contracts;
using Xunit.Abstractions;

namespace OCPI.JsonConverters.Serializing;

public class ForCredentials(ITestOutputHelper output) {
    private static readonly OcpiCredentials fullCredentials = 
        new OcpiCredentials {
            PartyId = "EXA",
            CountryCode = "BE",
            Token = "no-more-secrets",
            Url = "http://my.version.url",
            BusinessDetails = new() {
                Name = "My Business",
                Website = "my-business.com"
            },
            Roles = [
                new OcpiCredentialsRole {
                    Role = PartyRole.Emsp,
                    CountryCode = OCPI.CountryCode.Belgium,
                    PartyId = "EXA",
                    BusinessDetails = new() {
                        Name = "My Business",
                        Website = "my-business.com"
                    }
                }
            ]
        };
    
    [Fact]
    public void OcpiCredentials_Can_Be_Serialized_ForVersion211() {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new JsonSerdeExtraSettings(() => [ OcpiVersion.v2_1_1 ]));
        ConfigureOcpiJsonExtension.ConfigureJsonSerdes(options);

        var json = JsonNode.Parse(JsonSerializer.Serialize(fullCredentials, options));

        json.Should().NotBeNull();
        output.WriteLine(
            json!.ToJsonString(new JsonSerializerOptions { WriteIndented = true })
        );

        json!["token"]!.GetValue<string>().Should().Be("no-more-secrets");
        json!["url"]!.GetValue<string>().Should().Be("http://my.version.url");

        // v2.1.1 only
        json!["party_id"]!.GetValue<string>().Should().Be("EXA");
        json!["country_code"]!.GetValue<string>().Should().Be("BE");
        
        // v2.2.1 only
        json!["roles"].Should().BeNull();
    }
    
    [Fact]
    public void OcpiCredentials_Can_Be_Serialized_ForVersion221() {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new JsonSerdeExtraSettings(() => [ OcpiVersion.v2_2_1 ]));
        ConfigureOcpiJsonExtension.ConfigureJsonSerdes(options);

        var json = JsonNode.Parse(JsonSerializer.Serialize(fullCredentials, options));

        json.Should().NotBeNull();
        output.WriteLine(
            json!.ToJsonString(new JsonSerializerOptions { WriteIndented = true })
        );

        json!["token"]!.GetValue<string>().Should().Be("no-more-secrets");
        json!["url"]!.GetValue<string>().Should().Be("http://my.version.url");

        // v2.1.1 only
        json!["party_id"].Should().BeNull();
        json!["country_code"].Should().BeNull();
        
        // v2.2.1 only
        json!["roles"]!.AsArray().Should().HaveCount(1);
        json!["roles"]!.AsArray().First()!["role"]!.GetValue<string>().Should().Be("EMSP");
        json!["roles"]!.AsArray().First()!["party_id"]!.GetValue<string>().Should().Be("EXA");
        json!["roles"]!.AsArray().First()!["country_code"]!.GetValue<string>().Should().Be("BE");
    }
}