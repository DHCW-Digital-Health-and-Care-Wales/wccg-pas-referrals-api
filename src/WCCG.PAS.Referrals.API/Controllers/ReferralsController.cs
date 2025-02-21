using Hl7.Fhir.Model;
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
    [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(Bundle), ContentTypes = ["application/fhir+json"])]
    [SwaggerResponse(StatusCodes.Status400BadRequest, ContentTypes = ["text/plain", "application/json"])]
    [SwaggerResponse(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateReferral([SwaggerRequestBody(Required = true)] [FromBody] Bundle bundle)
    {
        _logger.CalledMethod(nameof(CreateReferral));

        var outputBundle = await _referralService.CreateReferralAsync(bundle);

        return Ok(outputBundle);
    }
}
