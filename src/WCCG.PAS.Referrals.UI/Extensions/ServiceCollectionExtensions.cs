using System.Diagnostics.CodeAnalysis;
using Azure.Identity;
using FluentValidation;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using WCCG.PAS.Referrals.UI.Configs;
using WCCG.PAS.Referrals.UI.Models;
using WCCG.PAS.Referrals.UI.Repositories;
using WCCG.PAS.Referrals.UI.Services;
using WCCG.PAS.Referrals.UI.Validators;

namespace WCCG.PAS.Referrals.UI.Extensions;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCosmosClient(this IServiceCollection services, IHostEnvironment environment)
    {
        var cosmosClientOptions = new CosmosClientOptions
        {
            SerializerOptions = new CosmosSerializationOptions { PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase }
        };

        if (environment.IsDevelopment())
            return services.AddSingleton(provider =>
            {
                var cosmosConfig = provider.GetService<IOptions<CosmosConfig>>()?.Value;
                return new CosmosClient(cosmosConfig?.DatabaseEndpoint, cosmosConfig?.AuthKey, cosmosClientOptions);
            });

        return services.AddSingleton(provider =>
        {
            var cosmosConfig = provider.GetService<IOptions<CosmosConfig>>()?.Value;
            return new CosmosClient(cosmosConfig?.DatabaseEndpoint, new DefaultAzureCredential(), cosmosClientOptions);
        });
    }

    public static IServiceCollection AddCosmosRepositories(this IServiceCollection services)
    {
        return services.AddSingleton<ICosmosRepository<Referral>>(provider =>
        {
            var cosmosConfig = provider.GetService<IOptions<CosmosConfig>>()?.Value;
            return new CosmosRepository<Referral>(provider.GetService<CosmosClient>()!, cosmosConfig!);
        });
    }

    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        return services.AddScoped<IReferralService, ReferralService>();
    }

    public static IServiceCollection AddValidators(this IServiceCollection services)
    {
        return services.AddScoped<IValidator<Referral>, ReferralValidator>();
    }
}
