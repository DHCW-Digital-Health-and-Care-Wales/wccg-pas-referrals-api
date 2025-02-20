using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;

namespace WCCG.PAS.Referrals.API.Converters;

[ExcludeFromCodeCoverage]
public class FhirBundleConverter : JsonConverter<Bundle>
{
    private readonly JsonSerializerOptions _options = new JsonSerializerOptions()
        .ForFhir(ModelInfo.ModelInspector)
        .UsingMode(DeserializerModes.BackwardsCompatible)
        .Pretty();

    public override Bundle? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return JsonSerializer.Deserialize<Bundle>(reader: ref reader, _options);
    }

    public override void Write(Utf8JsonWriter writer, Bundle value, JsonSerializerOptions options)
    {
        writer.WriteRawValue(JsonSerializer.Serialize(value, _options));
    }
}
