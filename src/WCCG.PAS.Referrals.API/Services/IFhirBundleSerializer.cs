using Hl7.Fhir.Model;

namespace WCCG.PAS.Referrals.API.Services;

public interface IFhirBundleSerializer
{
    Bundle Deserialize(string bundleString);
    string Serialize(Bundle value);
}
