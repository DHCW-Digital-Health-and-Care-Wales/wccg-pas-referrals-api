using Hl7.Fhir.Model;

namespace WCCG.PAS.Referrals.API.Services;

public interface IReferralService
{
    Task<Bundle> CreateReferralAsync(Bundle bundle);
}
