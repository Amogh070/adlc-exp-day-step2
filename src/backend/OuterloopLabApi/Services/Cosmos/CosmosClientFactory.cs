using Azure.Identity;
using Microsoft.Azure.Cosmos;
using OuterloopLabApi.Options;

namespace OuterloopLabApi.Services.Cosmos;

public sealed class CosmosClientFactory
{
    public CosmosClient Create(CosmosOptions options, DefaultAzureCredential credential)
    {
        // Token-based auth via Managed Identity.
        return new CosmosClient(options.CosmosDbUri, credential);
    }
}
