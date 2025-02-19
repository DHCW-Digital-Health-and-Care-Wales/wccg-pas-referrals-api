using System.Diagnostics.CodeAnalysis;

namespace WCCG.PAS.Referrals.API.Configuration;

[ExcludeFromCodeCoverage]
public class ManagedIdentityConfig
{
    public required string ClientId { get; set; }
}
