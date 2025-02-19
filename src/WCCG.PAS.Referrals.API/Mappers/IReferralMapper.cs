using Hl7.Fhir.Model;
using WCCG.PAS.Referrals.API.DbModels;

namespace WCCG.PAS.Referrals.API.Mappers;

public interface IReferralMapper
{
    ReferralDbModel? MapFromBundle(Bundle bundle);
}
