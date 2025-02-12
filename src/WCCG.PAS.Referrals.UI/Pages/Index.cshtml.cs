using Microsoft.AspNetCore.Mvc.RazorPages;
using WCCG.PAS.Referrals.UI.DbModels;
using WCCG.PAS.Referrals.UI.Services;

namespace WCCG.PAS.Referrals.UI.Pages;

public class IndexModel(IReferralService service) : PageModel
{
    public IEnumerable<ReferralDbModel> Referrals { get; set; } = [];

    public async Task OnGet()
    {
        Referrals = await service.GetAllAsync();
    }
}
