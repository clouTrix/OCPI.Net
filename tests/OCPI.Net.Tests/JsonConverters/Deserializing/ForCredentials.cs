using System.Text.Json;
using FluentAssertions;
using OCPI.Contracts;
using OCPI.Serdes.Json;

namespace OCPI.Tests.JsonConverters.Deserializing;

public class ForCredentials {
    private static string jsonStr = """
                                    { "party_id": "EXA",
                                      "country_code": "BE",
                                      "token": "no-more-secrets",
                                      "url": "http://my.version.url",
                                      "business_details": {
                                        "name": "my-business",
                                        "website": "http://my.business.com"
                                      },
                                      "roles": [{
                                        "role": "EMSP",
                                        "party_id": "EXA",
                                        "country_code": "BE",
                                        "business_details": {
                                          "name": "my-business",
                                          "website": "http://my.business.com"
                                        }
                                      }]
                                    }
                                    """;
    
    [Fact]
    public void OcpiCredentials_Can_Be_Deserialized_ForVersion211() {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new OcpiJsonConverterExtraSettings(() => [ OcpiVersion.v2_1_1 ]));
        ConfigureOcpiJsonExtension.ConfigureJsonSerdes(options);
        
        var creds = JsonSerializer.Deserialize<OcpiCredentials>(jsonStr, options);
        
        creds.Should().NotBeNull();
        creds!.Token.Should().Be("no-more-secrets");
        creds!.Url.Should().Be("http://my.version.url");

        // v2.1.1 only
        creds!.PartyId.Should().Be("EXA");
        creds!.CountryCode.Should().Be("BE");
        
        // v2.2.1 only
        creds!.Roles.Should().BeNull();
    }
    
    [Fact]
    public void OcpiCredentials_Can_Be_Deserialized_ForVersion221() {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new OcpiJsonConverterExtraSettings(() => [ OcpiVersion.v2_2_1 ]));
        ConfigureOcpiJsonExtension.ConfigureJsonSerdes(options);

        var creds = JsonSerializer.Deserialize<OcpiCredentials>(jsonStr, options);
        
        creds.Should().NotBeNull();
        creds!.Token.Should().Be("no-more-secrets");
        creds!.Url.Should().Be("http://my.version.url");

        // v2.1.1 only
        creds!.PartyId.Should().BeNull();
        creds!.CountryCode.Should().BeNull();
        
        // v2.2.1 only
        creds!.Roles.Should().HaveCount(1);
        creds!.Roles!.First()!.Role.Should().Be(OCPI.PartyRole.Emsp);
        creds!.Roles!.First()!.PartyId.Should().Be("EXA");
        creds!.Roles!.First()!.CountryCode.Should().Be(OCPI.CountryCode.Belgium);
        creds!.Roles!.First()!.BusinessDetails.Should().NotBeNull();
    }
}