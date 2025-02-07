using WCCG.PAS.Referrals.UI.Models;
using WCCG.PAS.Referrals.UI.Repositories;

namespace WCCG.PAS.Referrals.UI.Services;

public class ReferralService(ICosmosRepository<Referral> repository) : IReferralService
{
    public async Task<bool> UpsertAsync(Referral item)
    {
        return await repository.UpsertAsync(item);
    }

    public async Task<IEnumerable<Referral>> GetAllAsync()
    {
        return await repository.GetAllAsync();
    }

    public async Task<Referral> GetByIdAsync(string id)
    {
        return await repository.GetByIdAsync(id);
    }
}
