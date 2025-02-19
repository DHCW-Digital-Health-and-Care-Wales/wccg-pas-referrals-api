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
    public static void AddApplicationInsights(this IServiceCollection services, IHostEnvironment environment, string clientId)
    {
        services.AddApplicationInsightsTelemetry();

        services.Configure<TelemetryConfiguration>(config =>
        {
            if (environment.IsDevelopment())
            {
                config.SetAzureTokenCredential(new AzureCliCredential());
                return;
            }

            config.SetAzureTokenCredential(new ManagedIdentityCredential(clientId));
        });
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
        services.AddSingleton<IReferralCosmosRepository>(provider =>
        {
            var cosmosConfig = provider.GetService<IOptions<CosmosConfig>>()?.Value;
            return new ReferralCosmosRepository(provider.GetService<CosmosClient>()!, cosmosConfig!);
        });
    }

    public static void AddServices(this IServiceCollection services)
    {
        services.AddScoped<IReferralMapper, ReferralMapper>();
        services.AddScoped<IReferralService, ReferralService>();
        services.AddScoped<IFhirSerializer, FhirJsonSerializer>();
        services.AddScoped<IBundleFiller, BundleFiller>();
    }

    public static void AddValidators(this IServiceCollection services)
    {
        services.AddScoped<IValidator<ReferralDbModel>, ReferralDbModelValidator>();
    }
}
