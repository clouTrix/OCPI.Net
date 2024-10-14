using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using OCPI.Contracts;

namespace OCPI;

/// <summary>
/// 
/// </summary>
public static class ConfigureOcpiJsonExtension {
    /// <summary>
    /// 
    /// </summary>
    public static WebApplicationBuilder ConfigureOcpiJson(this WebApplicationBuilder builder) {
        builder.Services.ConfigureHttpJsonOptions(options => 
            ConfigureJsonSerdes(
                builder.Services.BuildServiceProvider(), 
                options.SerializerOptions
        ));

        return builder;
    }

    public static void ConfigureJsonSerdes(IServiceProvider sp, JsonSerializerOptions options) {
            options.PropertyNamingPolicy   = JsonNamingPolicy.CamelCase;
            options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            
            OcpiJsonConverter<T> ConverterFor<T>() where T: class => new (sp.GetService<ILogger<OcpiJsonConverter<T>>>());
            
            options.Converters.Add(ConverterFor<OcpiSession>());
            options.Converters.Add(ConverterFor<OcpiCdrLocation>());
            options.Converters.Add(ConverterFor<OcpiCdrToken>());
            options.Converters.Add(ConverterFor<OcpiSession>());
            options.Converters.Add(ConverterFor<OcpiSession>());
            options.Converters.Add(ConverterFor<OcpiLocation>());
            options.Converters.Add(ConverterFor<OcpiEvse>());
            options.Converters.Add(ConverterFor<OcpiConnector>());
            options.Converters.Add(ConverterFor<OcpiEndpoint>());
            options.Converters.Add(ConverterFor<OcpiVersionInfo>());
            options.Converters.Add(ConverterFor<OcpiVersionDetails>());
            options.Converters.Add(ConverterFor<OcpiCredentials>());
            options.Converters.Add(ConverterFor<OcpiBusinessDetails>());
            options.Converters.Add(ConverterFor<OcpiCredentialsRole>());
            
            options.Converters.Add(new JsonStringEnumMemberConverter());
            options.Converters.Add(new OcpiDateTimeConverter());
    }
}
