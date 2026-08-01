using Microsoft.Azure.Cosmos;
using OuterloopLabApi.Models;
using OuterloopLabApi.Options;

namespace OuterloopLabApi.Services.Cosmos;

public sealed class CosmosAuditRepository : IAuditRepository
{
    private readonly Container _container;

    public CosmosAuditRepository(CosmosClient cosmosClient, CosmosOptions options)
    {
        _container = cosmosClient.GetContainer(options.CosmosDbDatabase, options.CosmosDbContainer);
    }

    public Task CreateAsync(AuditRecord record, CancellationToken cancellationToken)
    {
        // Partition key = auditId
        return _container.CreateItemAsync(record, new PartitionKey(record.AuditId), cancellationToken: cancellationToken);
    }

    public async Task<AuditRecord?> GetByAuditIdAsync(string auditId, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _container.ReadItemAsync<AuditRecord>(auditId, new PartitionKey(auditId), cancellationToken: cancellationToken);
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }
}
