using System.Text.Json;
using System.Text.Json.Serialization;

namespace OCPI.Serdes.Json;

// This converter is not intended to do actual serialization/deserialization
// it's only use is to pass additional settings into the real JsonConverters.
public class OcpiJsonConverterExtraSettings(Func<IEnumerable<OcpiVersion>>? versionProvider): JsonConverter<OcpiJsonConverterExtraSettings> {
    public override OcpiJsonConverterExtraSettings? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => throw new NotImplementedException();
    public override void Write(Utf8JsonWriter writer, OcpiJsonConverterExtraSettings value, JsonSerializerOptions options)             => throw new NotImplementedException();

    // List of supported OCPI versions
    // The converter uses this to find out which OCPIDeprecated and OCPIIntroduced attributes
    // it needs to take into account when (de)serialising JSON data.
    public IEnumerable<OcpiVersion> Versions => versionProvider?.Invoke() ?? [];
}