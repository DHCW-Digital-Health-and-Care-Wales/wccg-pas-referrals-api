using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace WCCG.PAS.Referrals.API.Configuration;

[ExcludeFromCodeCoverage]
public class CosmosConfig
{
    public static string SectionName => "Cosmos";

    [Required]
    public required string DatabaseEndpoint { get; set; }

    [Required]
    public required string DatabaseName { get; set; }

    [Required]
    public required string ContainerName { get; set; }
}
