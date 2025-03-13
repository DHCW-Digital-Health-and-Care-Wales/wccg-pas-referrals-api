using WCCG.PAS.Referrals.API.DbModels;

namespace WCCG.PAS.Referrals.API.Repositories;

public interface IReferralCosmosRepository
{
    Task CreateReferralAsync(ReferralDbModel referralDbModel);
    Task<ReferralDbModel> GetReferralAsync(string referralId);
}
