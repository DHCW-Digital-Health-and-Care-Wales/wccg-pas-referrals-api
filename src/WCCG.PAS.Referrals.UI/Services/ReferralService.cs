using WCCG.PAS.Referrals.UI.DbModels;
using WCCG.PAS.Referrals.UI.Repositories;

namespace WCCG.PAS.Referrals.UI.Services;

public class ReferralService(ICosmosRepository<ReferralDbModel> repository) : IReferralService
{
    public async Task<bool> UpsertAsync(ReferralDbModel item)
    {
        return await repository.UpsertAsync(item);
    }

    public async Task<IEnumerable<ReferralDbModel>> GetAllAsync()
    {
        return await repository.GetAllAsync();
    }

    public async Task<ReferralDbModel> GetByIdAsync(string id)
    {
        return await repository.GetByIdAsync(id);
    }
}
