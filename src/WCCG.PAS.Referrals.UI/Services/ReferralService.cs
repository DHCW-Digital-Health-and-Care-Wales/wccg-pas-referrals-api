using WCCG.PAS.Referrals.UI.DbModels;
using WCCG.PAS.Referrals.UI.Repositories;

namespace WCCG.PAS.Referrals.UI.Services;

public class ReferralService : IReferralService
{
    private readonly ICosmosRepository<ReferralDbModel> _repository;

    public ReferralService(ICosmosRepository<ReferralDbModel> repository)
    {
        _repository = repository;
    }

    public async Task<bool> UpsertAsync(ReferralDbModel item)
    {
        return await _repository.UpsertAsync(item);
    }

    public async Task<IEnumerable<ReferralDbModel>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<ReferralDbModel> GetByIdAsync(string id)
    {
        return await _repository.GetByIdAsync(id);
    }
}
