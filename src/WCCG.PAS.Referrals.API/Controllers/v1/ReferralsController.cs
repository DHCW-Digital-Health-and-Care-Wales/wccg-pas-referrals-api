using System.Net.Mime;
using Asp.Versioning;
using Hl7.Fhir.Model;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
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
    private readonly IFhirBundleSerializer _fhirBundleSerializer;

    public ReferralsController(ILogger<ReferralsController> logger,
        IReferralService referralService,
        IFhirBundleSerializer fhirBundleSerializer)
    {
        _logger = logger;
        _referralService = referralService;
        _fhirBundleSerializer = fhirBundleSerializer;
    }

    [HttpPost("createReferral")]
    [Produces("application/fhir+json", Type = typeof(Bundle))]
    [Consumes("application/fhir+json", MediaTypeNames.Application.Json)]
    [SwaggerJsonRequest]
    [SwaggerOperation(Summary = "Create referral")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, ContentTypes = [MediaTypeNames.Application.Json])]
    [SwaggerResponse(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateReferral()
    {
        _logger.CalledMethod(nameof(CreateReferral));

        using var reader = new StreamReader(HttpContext.Request.Body);
        var bundleJson = await reader.ReadToEndAsync();

        var bundle = _fhirBundleSerializer.Deserialize(bundleJson);
        var outputBundle = await _referralService.CreateReferralAsync(bundle);

        var outputBundleJson = _fhirBundleSerializer.Serialize(outputBundle);
        return new ContentResult { Content = outputBundleJson, StatusCode = 200, ContentType = "application/fhir+json" };
    }
}
