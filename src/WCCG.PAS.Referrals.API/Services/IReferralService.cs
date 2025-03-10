namespace WCCG.PAS.Referrals.API.Services;

public interface IReferralService
{
    Task<string> CreateReferralAsync(string bundleJson);
}
