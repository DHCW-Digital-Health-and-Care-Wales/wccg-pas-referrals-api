using System.Diagnostics.CodeAnalysis;
using Microsoft.Azure.Cosmos;
using WCCG.PAS.Referrals.API.Configuration;
using WCCG.PAS.Referrals.API.DbModels;

namespace WCCG.PAS.Referrals.API.Repositories;

[ExcludeFromCodeCoverage]
public class ReferralCosmosRepository : IReferralCosmosRepository
{
    private readonly Container _container;

    public ReferralCosmosRepository(CosmosClient client, CosmosConfig config)
    {
        var database = client.GetDatabase(config.DatabaseName);
        _container = database.GetContainer(config.ContainerName);
    }

    public async Task CreateReferralAsync(ReferralDbModel referralDbModel)
    {
        await _container.CreateItemAsync(referralDbModel);
    }
}
