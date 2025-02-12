using System.Diagnostics.CodeAnalysis;
using Azure.Identity;
using FluentValidation;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using WCCG.PAS.Referrals.UI.Configs;
using WCCG.PAS.Referrals.UI.DbModels;
using WCCG.PAS.Referrals.UI.Repositories;
using WCCG.PAS.Referrals.UI.Services;
using WCCG.PAS.Referrals.UI.Validators;

namespace WCCG.PAS.Referrals.UI.Extensions;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static void AddApplicationInsights(this IServiceCollection services, IHostEnvironment environment, string clientId)
    {
        services.AddApplicationInsightsTelemetry();

        if (environment.IsDevelopment())
        {
            services.Configure<TelemetryConfiguration>(config => { config.SetAzureTokenCredential(new AzureCliCredential()); });
            return;
        }

        services.Configure<TelemetryConfiguration>(config => { config.SetAzureTokenCredential(new ManagedIdentityCredential(clientId)); });
    }

    public static void AddCosmosClient(this IServiceCollection services, IHostEnvironment environment)
    {
        var cosmosClientOptions = new CosmosClientOptions
        {
            SerializerOptions = new CosmosSerializationOptions { PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase }
        };

        if (environment.IsDevelopment())
        {
            services.AddSingleton(provider =>
            {
                var cosmosConfig = provider.GetService<IOptions<CosmosConfig>>()?.Value;
                return new CosmosClient(cosmosConfig?.DatabaseEndpoint, new AzureCliCredential(), cosmosClientOptions);
            });
            return;
        }

        services.AddSingleton(provider =>
        {
            var cosmosConfig = provider.GetService<IOptions<CosmosConfig>>()?.Value;
            var managedIdentityConfig = provider.GetService<IOptions<ManagedIdentityConfig>>()?.Value;

            return new CosmosClient(
                cosmosConfig?.DatabaseEndpoint,
                new ManagedIdentityCredential(managedIdentityConfig?.ClientId),
                cosmosClientOptions);
        });
    }

    public static void AddCosmosRepositories(this IServiceCollection services)
    {
        services.AddSingleton<ICosmosRepository<ReferralDbModel>>(provider =>
        {
            var cosmosConfig = provider.GetService<IOptions<CosmosConfig>>()?.Value;
            return new CosmosRepository<ReferralDbModel>(provider.GetService<CosmosClient>()!, cosmosConfig!);
        });
    }

    public static void AddServices(this IServiceCollection services)
    {
        services.AddScoped<IReferralService, ReferralService>();
    }

    public static void AddValidators(this IServiceCollection services)
    {
        services.AddScoped<IValidator<ReferralDbModel>, ReferralValidator>();
    }
}
