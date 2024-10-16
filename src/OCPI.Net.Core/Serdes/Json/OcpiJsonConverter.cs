using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.Extensions.Logging;

namespace OCPI.Serdes.Json;

public static class OcpiJsonConverter
{
    private static OcpiVersion DeprecatedFrom(this ICustomAttributeProvider self, OcpiVersion version)
        => self.GetCustomAttributes(typeof(OcpiDeprecatedAttribute), false)
            .Select(attr => ((OcpiDeprecatedAttribute)attr).Version)
            .Max(e => (OcpiVersion?)e) ?? version;

    private static OcpiVersion IntroducedFrom(this ICustomAttributeProvider self, OcpiVersion version)
        => self.GetCustomAttributes(typeof(OcpiIntroducedAttribute), false)
            .Select(attr => ((OcpiIntroducedAttribute)attr).Version)
            .Max(e => (OcpiVersion?)e) ?? version;

    private static bool IsVersionMarked(this ICustomAttributeProvider self)
        => self.IsDefined(typeof(OcpiDeprecatedAttribute), false) ||
           self.IsDefined(typeof(OcpiIntroducedAttribute), false);

    private static bool IsVersionMarked(this JsonPropertyInfo self)
        => self.AttributeProvider?.IsVersionMarked() ?? false;

    private static bool ShouldBeAllowed(this ICustomAttributeProvider self, OcpiVersion? version)
        => version is not null
              ? version.Value <= self.DeprecatedFrom(version.Value) && version >= self.IntroducedFrom(version.Value)
              : !self.IsVersionMarked();

    private static bool ShouldBeIgnored(this ICustomAttributeProvider self, OcpiVersion? version)
        => !self.ShouldBeAllowed(version);

    private static Action<JsonPropertyInfo> DisableSerialization(LazyLogging? logger = null)
        => self => {
            logger?.Debug("Disabling JSON serialization - name: {Name}, type: {Type}", () => [self.Name, self.PropertyType]);
            self.ShouldSerialize = (_, _) => false;
            self.Get = null;
            self.Set = null;
        };

    internal static Action<JsonTypeInfo> JsonIgnoreUnsupportedVersion(OcpiVersion? version, LazyLogging? logger = null)
        => typeInfo => {
            if (typeInfo.Kind != JsonTypeInfoKind.Object) return;
            
            logger?.Debug("Check JSON ignore for unsupported OCPI Version - version: {Version}", version);

            typeInfo.Properties
                .Where(IsVersionMarked)
                .Where(p => p.AttributeProvider?.ShouldBeIgnored(version) ?? false)
                .ForEach(
                    DisableSerialization(logger)
                 );
        };

    internal static IEnumerable<OcpiVersion> ForOcpiVersions(this JsonSerializerOptions self)
        => self.GetConverter(typeof(OcpiJsonConverterExtraSettings)) switch
                    {
                        OcpiJsonConverterExtraSettings conv => conv.Versions,
                        _ => []
                    };
}

/// <summary>
/// JSON Converter to (de)serialize OCPI.Contract data models.
/// This converter is configurable through injection of a JsonSerdeExtraSettings object into the JsonSerializerOptions
/// </summary>
public class OcpiJsonConverter<T>(ILogger<OcpiJsonConverter<T>>? sourceLogger = null) : JsonConverter<T> where T: class, new() { 
    private readonly LazyLogging logger = new LazyLogging(sourceLogger);

    private JsonSerializerOptions ModifiedOptions(JsonSerializerOptions options)
    {
        var opts = new JsonSerializerOptions(options) {
            TypeInfoResolver = new DefaultJsonTypeInfoResolver {
                Modifiers = {
                    OcpiJsonConverter.JsonIgnoreUnsupportedVersion(options.ForOcpiVersions().Max(v => (OcpiVersion?)v), logger)
                }
            }
        };
        opts.Converters.Remove(this);
        return opts;
    }

    /// <summary>
    /// 
    /// </summary>
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => (T?)JsonSerializer.Deserialize(ref reader, typeToConvert, ModifiedOptions(options));

    /// <summary>
    /// 
    /// </summary>
    public override void Write(Utf8JsonWriter writer, T sourceObj, JsonSerializerOptions options)
        => JsonSerializer.Serialize(writer, sourceObj, sourceObj.GetType(), ModifiedOptions(options));
}
