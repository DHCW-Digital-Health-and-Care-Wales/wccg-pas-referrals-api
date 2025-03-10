using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;

namespace WCCG.PAS.Referrals.API.Extensions;

[ExcludeFromCodeCoverage]
public static class JsonSerializerOptionsExtensions
{
    public static JsonSerializerOptions ForFhirExtended(this JsonSerializerOptions options)
    {
        return options.ForFhir(ModelInfo.ModelInspector)
            .UsingMode(DeserializerModes.BackwardsCompatible)
            .Pretty();
    }
}
