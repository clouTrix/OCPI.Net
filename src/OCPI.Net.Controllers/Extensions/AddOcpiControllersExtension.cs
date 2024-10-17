using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OCPI.Serdes.Json;
using OCPI.Services;

using static OCPI.ConfigureOcpiJsonExtension;

namespace OCPI;

public static class AddOcpiControllersExtension
{
    public static WebApplicationBuilder AddOcpiControllers(this WebApplicationBuilder builder) {
        builder.Services.AddControllers()
            .AddJsonOptions(options => {
                    var sp                  = builder.Services.BuildServiceProvider();
                    var httpContextAccessor = sp.GetService<IHttpContextAccessor>();
                    
                    // gets the supported OCPI versions from the controller attributes
                    options.JsonSerializerOptions.Converters.Add(
                        new OcpiJsonConverterExtraSettings(() => 
                               httpContextAccessor?.HttpContext?.GetEndpoint()?.Metadata.GetMetadata<OcpiEndpointAttribute>()?.Versions
                            ?? httpContextAccessor?.HttpContext?.GetEndpoint()?.Metadata.GetMetadata<OcpiVersionAttribute>()?.Versions
                            ?? []
                    ));
                    
                    ConfigureJsonSerdes(options.JsonSerializerOptions, sp);
                })
            .ConfigureApiBehaviorOptions(options => {
                    options.InvalidModelStateResponseFactory = context => {
                        var errors = context.ModelState.Values
                                                .SelectMany(x => x.Errors)
                                                .Select(x => x.ErrorMessage);

                        var exception = OcpiException.InvalidParameters("Failed to parse request data");
                        exception.Payload.AddData(new { errors });
                        throw exception;
                    };
            });

        builder.Services.AddScoped<PageResponseService>();

        return builder;
    }
}
