using System.Diagnostics.CodeAnalysis;
using Azure.Identity;
using FluentValidation;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using WCCG.PAS.Referrals.API.Configuration;
using WCCG.PAS.Referrals.API.DbModels;
using WCCG.PAS.Referrals.API.Helpers;
using WCCG.PAS.Referrals.API.Mappers;
using WCCG.PAS.Referrals.API.Repositories;
using WCCG.PAS.Referrals.API.Services;
using WCCG.PAS.Referrals.API.Validators;

namespace WCCG.PAS.Referrals.API.Extensions;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static void AddApplicationInsights(this IServiceCollection services, bool isDevelopmentEnvironment, string clientId)
    {
        services.AddApplicationInsightsTelemetry();
        var sp = services.BuildServiceProvider();
        services.Configure<TelemetryConfiguration>(config =>
        {
            if (isDevelopmentEnvironment)
            {
                config.SetAzureTokenCredential(new AzureCliCredential());
                return;
            }

            config.SetAzureTokenCredential(new ManagedIdentityCredential(clientId));
        });
    }

    public static void AddCosmosClient(this IServiceCollection services, bool isDevelopmentEnvironment)
    {
        var cosmosClientOptions = new CosmosClientOptions
        {
            SerializerOptions = new CosmosSerializationOptions { PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase }
        };

        if (isDevelopmentEnvironment)
        {
            services.AddSingleton(provider =>
            {
                var cosmosConfig = provider.GetRequiredService<IOptions<CosmosConfig>>().Value;
                return new CosmosClient(cosmosConfig?.DatabaseEndpoint, new AzureCliCredential(), cosmosClientOptions);
            });
            return;
        }

        services.AddSingleton(provider =>
        {
            var cosmosConfig = provider.GetRequiredService<IOptions<CosmosConfig>>().Value;
            var managedIdentityConfig = provider.GetRequiredService<IOptions<ManagedIdentityConfig>>().Value;

            return new CosmosClient(
                cosmosConfig?.DatabaseEndpoint,
                new ManagedIdentityCredential(managedIdentityConfig?.ClientId),
                cosmosClientOptions);
        });
    }

    public static void AddCosmosRepositories(this IServiceCollection services)
    {
        services.AddScoped<IReferralCosmosRepository>(provider =>
        {
            var cosmosConfig = provider.GetRequiredService<IOptions<CosmosConfig>>().Value;
            return new ReferralCosmosRepository(provider.GetRequiredService<CosmosClient>(), cosmosConfig);
        });
    }

    public static void AddServices(this IServiceCollection services)
    {
        services.AddScoped<IReferralMapper, ReferralMapper>();
        services.AddScoped<IReferralService, ReferralService>();
        services.AddScoped<IBundleFiller, BundleFiller>();
    }

    public static void AddValidators(this IServiceCollection services)
    {
        services.AddScoped<IValidator<ReferralDbModel>, ReferralDbModelValidator>();
    }
}
