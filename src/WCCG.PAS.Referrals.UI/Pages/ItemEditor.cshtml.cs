using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WCCG.PAS.Referrals.UI.DbModels;
using WCCG.PAS.Referrals.UI.Extensions;
using WCCG.PAS.Referrals.UI.Services;

namespace WCCG.PAS.Referrals.UI.Pages;

public class ItemEditorModel : PageModel
{
    private readonly IReferralService _referralService;
    private readonly IValidator<ReferralDbModel> _validator;
    private readonly ILogger<ItemEditorModel> _logger;

    public ItemEditorModel(IReferralService referralService, IValidator<ReferralDbModel> validator, ILogger<ItemEditorModel> logger)
    {
        _referralService = referralService;
        _validator = validator;
        _logger = logger;
    }

    [BindProperty]
    public required string? ReferralJson { get; set; }

    public bool? IsSaved { get; set; }
    public string? ErrorMessage { get; set; }

    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public async Task OnGet(string id)
    {
        var referral = await _referralService.GetByIdAsync(id);

        ReferralJson = JsonSerializer.Serialize(referral, _jsonOptions);
    }

    public async Task<IActionResult> OnPost()
    {
        if (ReferralJson is null)
        {
            return BadRequest();
        }

        var referral = DeserializeReferral();
        if (referral is null)
        {
            return Page();
        }

        var isValid = await IsReferralValidAsync(referral);
        if (!isValid)
        {
            return Page();
        }

        await UpsertReferralAsync(referral);
        return Page();
    }

    private ReferralDbModel? DeserializeReferral()
    {
        try
        {
            return JsonSerializer.Deserialize<ReferralDbModel>(ReferralJson!);
        }
        catch (JsonException ex)
        {
            _logger.FailedToDeserializeReferral(ex);
            HandleErrors(ex.Message);
        }

        return null;
    }

    private async Task<bool> IsReferralValidAsync(ReferralDbModel referral)
    {
        var validationResult = await _validator.ValidateAsync(referral);

        if (validationResult.IsValid)
        {
            return true;
        }

        var errors = validationResult.Errors.Select(x => x.ErrorMessage).ToArray();
        _logger.ReferralValidationFailed(string.Join(';', errors));
        HandleErrors(errors);

        return false;
    }

    private async Task UpsertReferralAsync(ReferralDbModel referral)
    {
        try
        {
            await _referralService.UpsertAsync(referral);
            IsSaved = true;
        }
        catch (Exception ex)
        {
            _logger.FailedToUpsertReferral(ex);
            HandleErrors(ex.Message);
        }
    }

    private void HandleErrors(params string[] errorMessages)
    {
        IsSaved = false;
        ErrorMessage = errorMessages.Aggregate((f, s) => f + "<br/>" + s);
    }
}
