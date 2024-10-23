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
    
    private static Func<PropertyInfo, bool> HasReadableValue(object? obj)
        => prop => prop.CanRead && prop.GetValue(obj, null) is not null;

    private static bool HasWritableValue(PropertyInfo prop)
        => prop.CanWrite;

    private static bool IsExtensionDataProperty(PropertyInfo prop)
        => prop.GetCustomAttribute<JsonExtensionDataAttribute>() is not null;

    private static Func<PropertyInfo, bool> AllowedForVersion(OcpiVersion? version)
        => prop => {
            OcpiVersion Deprecated() => prop.GetCustomAttribute<OcpiDeprecatedAttribute>()?.Version ?? version.Value;
            OcpiVersion Introduced() => prop.GetCustomAttribute<OcpiIntroducedAttribute>()?.Version ?? version.Value;
            bool IsVersionMarked() => version is not null && (Attribute.IsDefined(prop, typeof(OcpiDeprecatedAttribute)) || Attribute.IsDefined(prop, typeof(OcpiIntroducedAttribute)));

            bool ShouldBeIgnored() =>
                prop.GetCustomAttribute<JsonIgnoreAttribute>()?.Condition == JsonIgnoreCondition.Always;
            
            return version.HasValue && !ShouldBeIgnored() 
                            ? version <= Deprecated() && version >= Introduced()
                            : !IsVersionMarked();
        };

    private IEnumerable<OcpiVersion> SupportedVersionsOnCaller(JsonSerializerOptions options)
        => options.GetConverter(typeof(OcpiJsonConverterExtraSettings)) switch {
                OcpiJsonConverterExtraSettings conv => conv.Versions,
                                          _ => []
            };

    private static string PropertyName(PropertyInfo prop)
        => prop.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? prop.Name;

    private static IEnumerable<(string Name, PropertyInfo Property)> AllPropertiesRenamed(Type typeToConvert, JsonSerializerOptions options)
        => typeToConvert.GetProperties()
            .Select(prop =>
                (
                    options.PropertyNamingPolicy?.ConvertName(PropertyName(prop)) ?? PropertyName(prop),
                    prop
                )
            );
    
    private JsonObject FindUnknownFields(JsonNode json, Type typeToConvert, JsonSerializerOptions options)
    {
        var propertyNames = AllPropertiesRenamed(typeToConvert, options)
                                        .Select(e => e.Name)
                                        .ToList();

        var result = new JsonObject();
        json.AsObject()
            .ExceptBy(propertyNames, kv => kv.Key)
            .Where(kv => kv.Value is not null)
            .ForEach( kv =>
                result.Add(kv.Key, kv.Value!.DeepClone())
             );
        return result;
    } 
    
    /// <summary>
    /// 
    /// </summary>
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        var json = JsonNode.Parse(ref reader);
        if (json is null) return null;
        
        var highestVersion = SupportedVersionsOnCaller(options).Max(v => (OcpiVersion?)v);
        var result         = new Lazy<T>(() => new T());

        logger.Debug("Read JSON as object - type: {Type}, version: {Version}", () => [ typeToConvert, highestVersion ]);

        (PropertyInfo Property, object? Value) DefinePropertyValue((string Name, PropertyInfo Property) e)
            => ( e.Property,
                 IsExtensionDataProperty(e.Property)
                     ? FindUnknownFields(json, typeToConvert, options).Deserialize(e.Property.PropertyType)
                     : json[e.Name].Deserialize(e.Property.PropertyType, options)
               );

        AllPropertiesRenamed(typeToConvert, options)
            .Do(e => logger.Debug("Read property from JSON - type: {Type}, version: {Version}, property-name: {Name}, property-type: {Value}", () => [typeToConvert, highestVersion, e.Name, e.Property.PropertyType]))
            .Where(e => HasWritableValue(e.Property))
            .Where(e => AllowedForVersion(highestVersion)(e.Property))
            .Select(DefinePropertyValue)
            .Where(e => e.Value is not null)
            .Do(e => logger.Debug("Set property value from JSON - type: {Type}, version: {Version}, property-name: {Name}, property-type: {Type}, property-value: {Value}", () => [typeToConvert, highestVersion, e.Property.Name, e.Property.PropertyType, e.Value]))
            .ForEach(e =>
                e.Property.SetValue(result.Value, e.Value)
            );

        return result.IsValueCreated? result.Value : null;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void Write(Utf8JsonWriter writer, T sourceObj, JsonSerializerOptions options) {
        var highestVersion = SupportedVersionsOnCaller(options).Max(v => (OcpiVersion?)v);
        logger.Debug("Write object as JSON - type: {Type}, version: {Version}", () => [ sourceObj.GetType(), highestVersion ]);
        
        (string Name, object? Value) DefinePropertyValue((string Name, PropertyInfo Property) e)
            => ( e.Name,
                 e.Property.GetValue(sourceObj, null)
               );
        
        writer.WriteStartObject();
        AllPropertiesRenamed(sourceObj.GetType(), options)
            .Do(e => logger.Debug("Write property to JSON - type: {Type}, version: {Version}, property-name: {Name}, property-type: {Type}", () => [ sourceObj.GetType(), highestVersion, e.Name, e.Property.PropertyType ]))
            .Where(e => ! IsExtensionDataProperty(e.Property))
            .Where(e => HasReadableValue(sourceObj)(e.Property))
            .Where(e => AllowedForVersion(highestVersion)(e.Property))
            .Select(DefinePropertyValue)
            .Do(e => logger.Debug("Write property to JSON - type: {Type}, version: {Version}, property-name: {Name}, property-value: {Value}", () => [ sourceObj.GetType(), highestVersion, e.Name, e.Value ]))
            .ForEach(e => {
                writer.WritePropertyName(e.Name);
                JsonSerializer.Serialize(writer, e.Value, e.Value!.GetType(), options);
            });
        
        writer.WriteEndObject();
    }
}
