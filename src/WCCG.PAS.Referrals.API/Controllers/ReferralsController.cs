using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WCCG.PAS.Referrals.API.Extensions;
using WCCG.PAS.Referrals.API.Services;

namespace WCCG.PAS.Referrals.API.Controllers;

[ApiController]
[Route("[controller]")]
public class ReferralsController : ControllerBase
{
    private readonly ILogger<ReferralsController> _logger;
    private readonly IReferralService _referralService;

    public ReferralsController(ILogger<ReferralsController> logger, IReferralService referralService)
    {
        _logger = logger;
        _referralService = referralService;
    }

    [HttpPost("createReferral")]
    [SwaggerOperation(Summary = "Create referral")]
    [SwaggerResponse(StatusCodes.Status200OK, contentTypes: "application/json")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, contentTypes: ["application/json", "text/plain"])]
    [SwaggerResponse(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateReferral()
    {
        _logger.CalledMethod(nameof(CreateReferral));

        using var reader = new StreamReader(HttpContext.Request.Body);
        var jsonInput = await reader.ReadToEndAsync();

        var bundle = await _referralService.CreateReferralAsync(jsonInput);

        return new ContentResult
        {
            Content = bundle ?? jsonInput,
            ContentType = "application/json",
            StatusCode = bundle is not null
                ? StatusCodes.Status200OK
                : StatusCodes.Status400BadRequest
        };
    }
}
