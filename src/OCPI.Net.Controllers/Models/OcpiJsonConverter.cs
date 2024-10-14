using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace OCPI;

/// <summary>
/// JSON Converter to (de)serialize OCPI.Contract data models.
/// This converter is configurable through injection of a JsonSerdeExtraSettings object into the JsonSerializerOptions
/// </summary>
public class OcpiJsonConverter<T>(ILogger<OcpiJsonConverter<T>>? logger) : JsonConverter<T> where T: class {
    private static string PropertyName(PropertyInfo prop)
        => prop.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? prop.Name;

    private static Func<PropertyInfo, bool> HasReadableValue(object? obj)
        => prop => prop.CanRead && prop.GetValue(obj, null) is not null;

    private static Func<PropertyInfo, bool> AllowedForVersion(OcpiVersion? version)
        => prop => {
            OcpiVersion Deprecated() => prop.GetCustomAttribute<OcpiDeprecatedAttribute>()?.Version ?? version.Value;
            OcpiVersion Introduced() => prop.GetCustomAttribute<OcpiIntroducedAttribute>()?.Version ?? version.Value;
            bool IsVersionMarked() => Attribute.IsDefined(prop, typeof(OcpiDeprecatedAttribute)) || Attribute.IsDefined(prop, typeof(OcpiIntroducedAttribute));
            
            return version is not null
                    ? version <= Deprecated() && version >= Introduced()
                    : !IsVersionMarked();
        };

    private IEnumerable<OcpiVersion> SupportedVersionsOnCaller(JsonSerializerOptions options)
        => options.GetConverter(typeof(JsonSerdeExtraSettings)) switch {
                JsonSerdeExtraSettings conv => conv.Versions,
                                          _ => []
            };
    
    /// <summary>
    /// 
    /// </summary>
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        JsonSerializerOptions ModifiedOptions() {
            var modifiedOptions = new JsonSerializerOptions(options);
            modifiedOptions.Converters.Remove(this);
            return modifiedOptions;
        }
        
        using var jsonDoc = JsonDocument.ParseValue(ref reader);

        //TODO: Read from OCPI version specific JSON for changed property definitions (e.g. OcpiSession::TotalCost)

        logger?.LogDebug("Read JSON as Object - type: {Type}", typeToConvert);
        return (T?)jsonDoc.Deserialize(typeToConvert, ModifiedOptions());
    }

    /// <summary>
    /// 
    /// </summary>
    public override void Write(Utf8JsonWriter writer, T sourceObj, JsonSerializerOptions options) {
        var highestVersion = SupportedVersionsOnCaller(options).Max(v => (OcpiVersion?)v);
        logger?.LogDebug("Write object as JSON - type: {Type}, version: {Version}", sourceObj.GetType(), highestVersion);
        
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
                logger?.LogDebug("Add property to JSON - type: {Type}, version: {Version}, name: {PropertyName}, value: {PropertyValue}", sourceObj.GetType(), highestVersion, nv.Name, nv.Value);
                writer.WritePropertyName(nv.Name);
                JsonSerializer.Serialize(writer, nv.Value, nv.Value!.GetType(), options);
            });
        
        writer.WriteEndObject();
    }
}
