using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;

namespace WCCG.PAS.Referrals.API.Helpers;

[ExcludeFromCodeCoverage]
public class FhirJsonSerializer : IFhirSerializer
{
    private readonly JsonSerializerOptions _options = new JsonSerializerOptions()
        .ForFhir(ModelInfo.ModelInspector)
        .UsingMode(DeserializerModes.BackwardsCompatible);

    public T? Deserialize<T>(string inputString)
    {
        return JsonSerializer.Deserialize<T>(inputString, _options);
    }

    public string Serialize<T>(T value)
    {
        return JsonSerializer.Serialize(value, _options);
    }
}
