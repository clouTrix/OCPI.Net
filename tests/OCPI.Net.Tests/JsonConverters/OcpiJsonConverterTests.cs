using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using OCPI.Contracts;
using Xunit.Abstractions;

namespace OCPI.JsonConverters;

public class OcpiJsonConverterTests(ITestOutputHelper output)
{
    private static OcpiCredentials FullObject = new OcpiCredentials {
            Token = "no-more-secrets",
            Url = "http://my.version.url",
            PartyId = "PTY",
            CountryCode = "BE",
            BusinessDetails = new OcpiBusinessDetails
            {
                Name = "Me",
                Website = "www.mine.be",
            },
            Roles = [
                new OcpiCredentialsRole
                {
                    CountryCode = OCPI.CountryCode.Belgium,
                    PartyId = "PTY",
                    Role = PartyRole.Cpo,
                    BusinessDetails = new OcpiBusinessDetails
                    {
                        Name = "Me",
                        Website = "www.mine.be",
                    }
                }
            ]
        };

    private readonly ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddXunit(output));

    private JsonSerializerOptions DefaultOptions(Action<JsonSerializerOptions> configure)
    {
        var opts = new JsonSerializerOptions
        {
            Converters =
            {
                new JsonStringEnumMemberConverter(),
                new OcpiDateTimeConverter(),
                new OcpiJsonConverter<OcpiCredentials>(factory.CreateLogger<OcpiJsonConverter<OcpiCredentials>>())
            }
        };

        configure(opts);
        return opts;
    }

    [Fact]
    public void JsonConverter_SerializesSelectedVersion_When_DeprecatedVersionGiven()
    {
        var options = DefaultOptions(c =>
            c.Converters.Add(new JsonSerdeExtraSettings(() => [OcpiVersion.v2_1_1]))
        );
        
        var json = JsonSerializer.SerializeToNode(FullObject, options);

        json.Should().NotBeNull();
        json!["token"]!.GetValue<string>().Should().Be("no-more-secrets");
        json!["url"]!.GetValue<string>().Should().Be("http://my.version.url");
        json!["party_id"]!.GetValue<string>().Should().Be("PTY");
        json!["country_code"]!.GetValue<string>().Should().Be("BE");
        json!["business_details"]!["name"]!.GetValue<string>().Should().Be("Me");
        json!["business_details"]!["website"]!.GetValue<string>().Should().Be("www.mine.be");
        json!["business_details"]!["logo"].Should().BeNull();
    }

    [Fact]
    public void JsonConverter_SerializesSelectedVersion_When_IntroducedVersionGiven()
    {
        var options = DefaultOptions(c =>
            c.Converters.Add(new JsonSerdeExtraSettings(() => [OcpiVersion.v2_2_1]))
        );
        
        var json = JsonSerializer.SerializeToNode(FullObject, options);

        json.Should().NotBeNull();
        json!["token"]!.GetValue<string>().Should().Be("no-more-secrets");
        json!["url"]!.GetValue<string>().Should().Be("http://my.version.url");
        json!["party_id"].Should().BeNull();            // specific upto v2.1.1
        json!["country_code"].Should().BeNull();        // specific upto v2.1.1
        json!["business_details"].Should().BeNull();    // specific upto v2.1.1
        json!["roles"]!.AsArray().Should().HaveCount(1);
        
        json!["roles"]!.AsArray().First()["country_code"]!.GetValue<string>().Should().Be("BE");
        json!["roles"]!.AsArray().First()["party_id"]!.GetValue<string>().Should().Be("PTY");
        json!["roles"]!.AsArray().First()["business_details"]!["name"]!.GetValue<string>().Should().Be("Me");
        json!["roles"]!.AsArray().First()["business_details"]!["website"]!.GetValue<string>().Should().Be("www.mine.be");
        json!["roles"]!.AsArray().First()["business_details"]!["logo"].Should().BeNull();
    }
    
    [Fact]
    public void JsonConverter_SerializesHighestVersion_When_NoVersionGiven()
    {
        var options = DefaultOptions(c =>
            c.Converters.Add(new JsonSerdeExtraSettings(() => []))
        );

        var json = JsonSerializer.SerializeToNode(FullObject, options);

        json.Should().NotBeNull();
        json!["token"]!.GetValue<string>().Should().Be("no-more-secrets");
        json!["url"]!.GetValue<string>().Should().Be("http://my.version.url");
        json!["party_id"].Should().BeNull();            // specific upto v2.1.1
        json!["country_code"].Should().BeNull();        // specific upto v2.1.1
        json!["business_details"].Should().BeNull();    // specific upto v2.1.1
        json!["roles"].Should().BeNull();               // specific from v2.2.1
    }
}