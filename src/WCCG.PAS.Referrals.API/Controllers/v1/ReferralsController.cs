using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using WCCG.PAS.Referrals.API.Constants;
using WCCG.PAS.Referrals.API.Extensions;
using WCCG.PAS.Referrals.API.Services;
using WCCG.PAS.Referrals.API.Swagger;

namespace WCCG.PAS.Referrals.API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ReferralsController : ControllerBase
{
    private readonly ILogger<ReferralsController> _logger;
    private readonly IReferralService _referralService;

    public ReferralsController(ILogger<ReferralsController> logger, IReferralService referralService)
    {
        _logger = logger;
        _referralService = referralService;
    }

    [HttpPost]
    [SwaggerCreateReferralRequest]
    public async Task<IActionResult> CreateReferral()
    {
        _logger.CalledMethod(nameof(CreateReferral));

        using var reader = new StreamReader(HttpContext.Request.Body);
        var bundleJson = await reader.ReadToEndAsync();

        var outputBundleJson = await _referralService.CreateReferralAsync(bundleJson);

        return new ContentResult
        {
            Content = outputBundleJson,
            StatusCode = 200,
            ContentType = FhirConstants.FhirMediaType
        };
    }

    [HttpGet("{referralId}")]
    [SwaggerGetReferralRequest]
    public async Task<IActionResult> GetReferral(string referralId)
    {
        _logger.CalledMethod(nameof(GetReferral));

        var outputBundleJson = await _referralService.GetReferralAsync(referralId);

        return new ContentResult
        {
            Content = outputBundleJson,
            StatusCode = 200,
            ContentType = FhirConstants.FhirMediaType
        };
    }
}
