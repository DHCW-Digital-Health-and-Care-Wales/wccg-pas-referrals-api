using Hl7.Fhir.Model;
using WCCG.PAS.Referrals.API.DbModels;

namespace WCCG.PAS.Referrals.API.Helpers;

public interface IBundleCreator
{
    Bundle CreateBundle(ReferralDbModel referralDbModel);
}
