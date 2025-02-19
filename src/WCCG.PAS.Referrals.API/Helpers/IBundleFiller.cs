using Hl7.Fhir.Model;
using WCCG.PAS.Referrals.API.DbModels;

namespace WCCG.PAS.Referrals.API.Helpers;

public interface IBundleFiller
{
    void AdjustBundleWithDbModelData(Bundle bundle, ReferralDbModel dbModel);
}
