using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace WCCG.PAS.Referrals.API.Configuration;

[ExcludeFromCodeCoverage]
public class ManagedIdentityConfig
{
    public static string SectionName => "ManagedIdentity";

    [Required]
    public required string ClientId { get; set; }
}
