using Microsoft.AspNetCore.Mvc.RazorPages;
using WCCG.PAS.Referrals.UI.DbModels;
using WCCG.PAS.Referrals.UI.Services;

namespace WCCG.PAS.Referrals.UI.Pages;

public class IndexModel : PageModel
{
    private readonly IReferralService _service;

    public IndexModel(IReferralService service)
    {
        _service = service;
    }

    public IEnumerable<ReferralDbModel> Referrals { get; set; } = [];

    public async Task OnGet()
    {
        Referrals = await _service.GetAllAsync();
    }
}
