using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace OCPI.Serdes.Json;

/// <summary>
/// JSON Converter to (de)serialize OCPI.Contract data models.
/// This converter is configurable through injection of a JsonSerdeExtraSettings object into the JsonSerializerOptions
/// </summary>
public class OcpiJsonConverter<T>(ILogger<OcpiJsonConverter<T>>? sourceLogger = null) : JsonConverter<T> where T: class, new()
{ 
    private readonly LazyLogging logger = new LazyLogging(sourceLogger);
    
    private static string PropertyName(PropertyInfo prop)
        => prop.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? prop.Name;

    private static Func<PropertyInfo, bool> HasReadableValue(object? obj)
        => prop => prop.CanRead && prop.GetValue(obj, null) is not null;

    private static bool HasWritableValue(PropertyInfo prop)
        => prop.CanWrite;

    private static Func<PropertyInfo, bool> AllowedForVersion(OcpiVersion? version)
        => prop => {
            OcpiVersion Deprecated() => prop.GetCustomAttribute<OcpiDeprecatedAttribute>()?.Version ?? version.Value;
            OcpiVersion Introduced() => prop.GetCustomAttribute<OcpiIntroducedAttribute>()?.Version ?? version.Value;
            bool IsVersionMarked() => Attribute.IsDefined(prop, typeof(OcpiDeprecatedAttribute)) || Attribute.IsDefined(prop, typeof(OcpiIntroducedAttribute));

            bool ShouldBeIgnored() =>
                prop.GetCustomAttribute<JsonIgnoreAttribute>()?.Condition == JsonIgnoreCondition.Always;
            
            return !ShouldBeIgnored() && version.HasValue
                            ? version <= Deprecated() && version >= Introduced()
                            : !IsVersionMarked();
        };

    private IEnumerable<OcpiVersion> SupportedVersionsOnCaller(JsonSerializerOptions options)
        => options.GetConverter(typeof(OcpiJsonConverterExtraSettings)) switch {
                OcpiJsonConverterExtraSettings conv => conv.Versions,
                                          _ => []
            };
    
    /// <summary>
    /// 
    /// </summary>
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        var json = JsonNode.Parse(ref reader);
        if (json is null) return null;
        
        var highestVersion = SupportedVersionsOnCaller(options).Max(v => (OcpiVersion?)v);
        var result         = new Lazy<T>(() => new T());

        logger.LogDebug("Read JSON as object - type: {Type}, version: {Version}", () => [ typeToConvert, highestVersion ]);
        
        typeToConvert.GetProperties()
            .Where(HasWritableValue)
            .Where(AllowedForVersion(highestVersion))
            //TODO: do we also need to take other JSON Attributes into account? (e.g. JsonIgnore, ...)
            .Select(prop =>
                (
                    Name : options.PropertyNamingPolicy?.ConvertName(PropertyName(prop)) ?? PropertyName(prop),
                    Property: prop
                )
             )
            .ToList()
            .ForEach(np => {
                logger.LogDebug("Read property from JSON - type: {Type}, version: {Version}, property-name: {Name}, property-type: {Value}", () => [ typeToConvert, highestVersion, np.Name, np.Property.PropertyType ]);
                np.Property.SetValue(
                    result.Value,
                    json[np.Name].Deserialize(np.Property.PropertyType, options)
                );
            });

        return result.IsValueCreated? result.Value : null;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void Write(Utf8JsonWriter writer, T sourceObj, JsonSerializerOptions options) {
        var highestVersion = SupportedVersionsOnCaller(options).Max(v => (OcpiVersion?)v);
        logger.LogDebug("Write object as JSON - type: {Type}, version: {Version}", () => [ sourceObj.GetType(), highestVersion ]);
        
        writer.WriteStartObject();
        sourceObj.GetType().GetProperties()
            .Where(HasReadableValue(sourceObj))
            .Where(AllowedForVersion(highestVersion))
            //TODO: do we also need to take other JSON Attributes into account? (e.g. JsonIgnore, ...)
            .Select(prop =>
                (
                    Name : options.PropertyNamingPolicy?.ConvertName(PropertyName(prop)) ?? PropertyName(prop),
                    Value: prop.GetValue(sourceObj, null)
                )
            )
            .ToList()
            .ForEach(nv => {
                logger.LogDebug("Write property to JSON - type: {Type}, version: {Version}, name: {PropertyName}, value: {PropertyValue}", () => [ sourceObj.GetType(), highestVersion, nv.Name, nv.Value ]);
                writer.WritePropertyName(nv.Name);
                JsonSerializer.Serialize(writer, nv.Value, nv.Value!.GetType(), options);
            });
        
        writer.WriteEndObject();
    }
}
