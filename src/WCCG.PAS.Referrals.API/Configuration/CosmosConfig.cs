using System.Diagnostics.CodeAnalysis;

namespace WCCG.PAS.Referrals.API.Configuration;

[ExcludeFromCodeCoverage]
public class CosmosConfig
{
    public required string DatabaseEndpoint { get; set; }
    public required string DatabaseName { get; set; }
    public required string ContainerName { get; set; }
}
