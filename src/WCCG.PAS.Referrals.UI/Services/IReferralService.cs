using WCCG.PAS.Referrals.UI.DbModels;

namespace WCCG.PAS.Referrals.UI.Services;

public interface IReferralService
{
    Task<bool> UpsertAsync(ReferralDbModel item);
    Task<IEnumerable<ReferralDbModel>> GetAllAsync();
    Task<ReferralDbModel> GetByIdAsync(string id);
}
