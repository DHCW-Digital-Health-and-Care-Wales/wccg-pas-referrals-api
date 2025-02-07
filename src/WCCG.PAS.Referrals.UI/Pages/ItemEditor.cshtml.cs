using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using WCCG.PAS.Referrals.UI.Models;
using WCCG.PAS.Referrals.UI.Services;

namespace WCCG.PAS.Referrals.UI.Pages;

public class ItemEditorModel(IReferralService referralService) : PageModel
{
    public Referral Referral { get; set; }

    public bool? IsSaved { get; set; }
    public string ErrorMessage { get; set; }

    public string ReferralJson => JsonConvert.SerializeObject(Referral, Formatting.Indented);

    public async Task OnGet(string id)
    {
        Referral = await referralService.GetByIdAsync(id);
    }

    public async Task OnPost(string id, string referralJson)
    {
        try
        {
            Referral = JsonConvert.DeserializeObject<Referral>(referralJson);
            await referralService.UpsertAsync(Referral);
            IsSaved = true;
        }
        catch (Exception ex)
        {
            Referral = await referralService.GetByIdAsync(id);
            IsSaved = false;
            ErrorMessage = ex.Message;
        }
    }
}
