using System.Diagnostics.CodeAnalysis;
using Asp.Versioning;
using Azure.Core;
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
    public static void AddApplicationInsights(this IServiceCollection services, bool isDevelopmentEnvironment, IConfiguration configuration)
    {
        var appInsightsConnectionString = configuration.GetRequiredSection(ApplicationInsightsConfig.SectionName)
            .GetValue<string>(nameof(ApplicationInsightsConfig.ConnectionString));

        services.AddApplicationInsightsTelemetry(options => options.ConnectionString = appInsightsConnectionString);
        services.Configure<TelemetryConfiguration>(config =>
        {
            if (isDevelopmentEnvironment)
            {
                config.SetAzureTokenCredential(new AzureCliCredential());
                return;
            }

            var clientId = configuration.GetRequiredSection(ManagedIdentityConfig.SectionName)
                .GetValue<string>(nameof(ManagedIdentityConfig.ClientId));
            config.SetAzureTokenCredential(new ManagedIdentityCredential(clientId));
        });
    }

    public static void AddCosmosClient(this IServiceCollection services, bool isDevelopmentEnvironment, IConfiguration configuration)
    {
        var cosmosClientOptions = new CosmosClientOptions
        {
            SerializerOptions = new CosmosSerializationOptions
            {
                PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase,
                IgnoreNullValues = false
            },
            ConnectionMode = ConnectionMode.Gateway, // TODO: Temporary workaround
        };

        var cosmosConfigSection = configuration.GetRequiredSection(CosmosConfig.SectionName);
        var cosmosEndpoint = cosmosConfigSection.GetValue<string>(nameof(CosmosConfig.DatabaseEndpoint));
        var cosmosDatabaseName = cosmosConfigSection.GetValue<string>(nameof(CosmosConfig.DatabaseName));
        var cosmosContainerName = cosmosConfigSection.GetValue<string>(nameof(CosmosConfig.ContainerName));

        TokenCredential tokenCredential;
        if (isDevelopmentEnvironment)
        {
            tokenCredential = new AzureCliCredential();
        }
        else
        {
            var managedIdentityConfig = configuration.GetRequiredSection(ManagedIdentityConfig.SectionName);
            var clientId = managedIdentityConfig.GetValue<string>(nameof(ManagedIdentityConfig.ClientId));

            tokenCredential = new ManagedIdentityCredential(clientId);
        }

        var cosmosClient = CosmosClient.CreateAndInitializeAsync(
            cosmosEndpoint,
            tokenCredential,
            [(cosmosDatabaseName, cosmosContainerName)],
            cosmosClientOptions);
        // Warning: Potentially missing GetAwaiter().GetResult()

        services.AddSingleton(_ => cosmosClient.Result);
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
        services.AddScoped<IBundleCreator, BundleCreator>();
    }

    public static void AddValidators(this IServiceCollection services)
    {
        services.AddScoped<IValidator<ReferralDbModel>, ReferralDbModelValidator>();
    }

    public static void AddVersioning(this IServiceCollection services)
    {
        services.AddApiVersioning(options => { options.DefaultApiVersion = new ApiVersion(1, 0); })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });
    }
}
