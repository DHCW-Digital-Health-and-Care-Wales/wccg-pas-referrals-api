using WCCG.PAS.Referrals.UI.Models;

namespace WCCG.PAS.Referrals.UI.Services;

public interface IReferralService
{
    Task<bool> UpsertAsync(Referral item);
    Task<IEnumerable<Referral>> GetAllAsync();
    Task<Referral> GetByIdAsync(string id);
}
