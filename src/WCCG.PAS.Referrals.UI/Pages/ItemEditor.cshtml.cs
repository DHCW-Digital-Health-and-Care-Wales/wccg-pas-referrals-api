using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WCCG.PAS.Referrals.UI.Models;
using WCCG.PAS.Referrals.UI.Services;

namespace WCCG.PAS.Referrals.UI.Pages;

public class ItemEditorModel(IReferralService referralService, IValidator<Referral> validator) : PageModel
{
    [BindProperty]
    public required string ReferralJson { get; set; }

    public required string ReferralId { get; set; }
    public bool? IsSaved { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;

    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public async Task OnGet(string id)
    {
        var referral = await referralService.GetByIdAsync(id);

        ReferralId = referral.Id!;
        ReferralJson = JsonSerializer.Serialize(referral, _jsonOptions);
    }

    public async Task OnPost()
    {
        Referral referral;
        try
        {
            referral = JsonSerializer.Deserialize<Referral>(ReferralJson)!;
        }
        catch (JsonException ex)
        {
            HandleErrors(ex.Message);
            return;
        }

        var validationResult = await validator.ValidateAsync(referral);

        if (!validationResult.IsValid)
        {
            HandleErrors(validationResult.Errors.Select(x => x.ErrorMessage).ToArray());
            return;
        }

        try
        {
            await referralService.UpsertAsync(referral);
            IsSaved = true;
        }
        catch (Exception ex)
        {
            HandleErrors(ex.Message);
        }
    }

    private void HandleErrors(params string[] errorMessages)
    {
        IsSaved = false;
        ErrorMessage = errorMessages.Aggregate((f, s) => f + "<br/>" + s);
    }
}
