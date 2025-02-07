using System.Diagnostics.CodeAnalysis;
using Microsoft.Azure.Cosmos;
using WCCG.PAS.Referrals.UI.Configs;
using WCCG.PAS.Referrals.UI.Models;
using WCCG.PAS.Referrals.UI.Repositories;
using WCCG.PAS.Referrals.UI.Services;

namespace WCCG.PAS.Referrals.UI.Extensions;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCosmosClient(this IServiceCollection services, IConfiguration configuration)
    {
        var cosmosClientOptions = new CosmosClientOptions
        {
            SerializerOptions = new CosmosSerializationOptions { PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase }
        };

#if DEBUG
        var authKey = configuration.GetValue<string>("Cosmos:AuthKey");
        var dbEndpoint = configuration.GetValue<string>("Cosmos:DbEndpoint");
        services.AddSingleton(_ => new CosmosClient(dbEndpoint, authKey, cosmosClientOptions));
#else
        var dbEndpoint = configuration.GetValue<string>("Cosmos::DbEndpoint");
        services.AddSingleton(_ => new CosmosClient(dbEndpoint, new DefaultAzureCredential(), cosmosClientOptions));
#endif
        return services;
    }

    public static IServiceCollection AddCosmosRepositories(this IServiceCollection services, IConfiguration configuration)
    {
        var referralsCosmosConfig = configuration.GetSection("Cosmos:Referrals").Get<CosmosConfig>();

        services.AddSingleton<ICosmosRepository<Referral>>(s =>
            new CosmosRepository<Referral>(s.GetService<CosmosClient>(), referralsCosmosConfig));

        return services;
    }

    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddTransient<IReferralService, ReferralService>();

        return services;
    }
}
