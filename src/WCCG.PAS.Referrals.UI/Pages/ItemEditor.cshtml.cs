using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WCCG.PAS.Referrals.UI.DbModels;
using WCCG.PAS.Referrals.UI.Services;

namespace WCCG.PAS.Referrals.UI.Pages;

public class ItemEditorModel
    (IReferralService referralService, IValidator<ReferralDbModel> validator, ILogger<ItemEditorModel> logger) : PageModel
{
    [BindProperty]
    public required string ReferralJson { get; set; }

    public required string ReferralId { get; set; }
    public bool? IsSaved { get; set; }
    public string? ErrorMessage { get; set; }

    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public async Task OnGet(string id)
    {
        var referral = await referralService.GetByIdAsync(id);

        ReferralId = referral.Id!;
        ReferralJson = JsonSerializer.Serialize(referral, _jsonOptions);
    }

    public async Task OnPost()
    {
        var referral = DeserializeReferral();
        if (referral is null)
        {
            return;
        }

        var isValid = await IsReferralValidAsync(referral);
        if (!isValid)
        {
            return;
        }

        await UpsertReferralAsync(referral);
    }

    private ReferralDbModel? DeserializeReferral()
    {
        try
        {
            return JsonSerializer.Deserialize<ReferralDbModel>(ReferralJson)!;
        }
        catch (JsonException ex)
        {
            logger.LogError(ex.Message);
            HandleErrors(ex.Message);
        }

        return null;
    }

    private async Task<bool> IsReferralValidAsync(ReferralDbModel referral)
    {
        var validationResult = await validator.ValidateAsync(referral);

        if (validationResult.IsValid)
        {
            return true;
        }

        var errors = validationResult.Errors.Select(x => x.ErrorMessage).ToArray();
        logger.LogError($"Validation errors: {string.Join(';', errors)}");
        HandleErrors(errors);

        return false;
    }

    private async Task UpsertReferralAsync(ReferralDbModel referral)
    {
        try
        {
            await referralService.UpsertAsync(referral);
            IsSaved = true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            HandleErrors(ex.Message);
        }
    }

    private void HandleErrors(params string[] errorMessages)
    {
        IsSaved = false;
        ErrorMessage = errorMessages.Aggregate((f, s) => f + "<br/>" + s);
    }
}
