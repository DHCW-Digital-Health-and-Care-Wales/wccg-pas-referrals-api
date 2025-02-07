using Microsoft.AspNetCore.Mvc.RazorPages;
using WCCG.PAS.Referrals.UI.Models;
using WCCG.PAS.Referrals.UI.Services;

namespace WCCG.PAS.Referrals.UI.Pages;

public class CosmosViewerModel(IReferralService service) : PageModel
{
    public IEnumerable<Referral> Referrals { get; set; }

    public async Task OnGet()
    {
        Referrals = await service.GetAllAsync();
    }
}
