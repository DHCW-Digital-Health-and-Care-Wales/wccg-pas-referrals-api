using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace WCCG.PAS.Referrals.API.Configuration;

[ExcludeFromCodeCoverage]
public class BundleCreationConfig
{
    public static string SectionName => "BundleCreation";

    [Required]
    public required string EReferralsBaseUrl { get; set; }

    [Required]
    public required string EReferralsCreateReferralEndpoint { get; set; }

    [Required]
    public required string DentalUiBaseUrl { get; set; }
}
