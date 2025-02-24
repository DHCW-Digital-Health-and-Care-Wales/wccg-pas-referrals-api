using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;

namespace WCCG.PAS.Referrals.API.Services;

[ExcludeFromCodeCoverage]
public class FhirBundleJsonSerializer : IFhirBundleSerializer
{
    private readonly JsonSerializerOptions _options = new JsonSerializerOptions()
        .ForFhir(ModelInfo.ModelInspector)
        .UsingMode(DeserializerModes.BackwardsCompatible)
        .Pretty();

    public Bundle Deserialize(string bundleString)
    {
        return JsonSerializer.Deserialize<Bundle>(bundleString, _options)!;
    }

    public string Serialize(Bundle value)
    {
        return JsonSerializer.Serialize(value, _options);
    }
}
