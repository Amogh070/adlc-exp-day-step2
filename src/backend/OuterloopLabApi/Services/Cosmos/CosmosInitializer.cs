using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.CosmosDB;
using Microsoft.Azure.Cosmos;
using OuterloopLabApi.Options;

namespace OuterloopLabApi.Services.Cosmos;

public sealed class CosmosInitializer
{
    private readonly CosmosOptions _options;
    private readonly DefaultAzureCredential _credential;
    private readonly CosmosClientFactory _clientFactory;
    private readonly CosmosClient _cosmosClient;

    public CosmosInitializer(CosmosOptions options, DefaultAzureCredential credential, CosmosClientFactory clientFactory)
    {
        _options = options;
        _credential = credential;
        _clientFactory = clientFactory;
        _cosmosClient = _clientFactory.Create(_options, _credential);
    }

    public async Task InitializeAsync()
    {
        await TryArmProvisionAsync();

        // Data-plane must be token-authenticated create-if-not-exists. If it fails, startup must fail.
        await _cosmosClient.CreateDatabaseIfNotExistsAsync(_options.CosmosDbDatabase, cancellationToken: CancellationToken.None);
        await EnsureContainerAsync();
    }

    private async Task EnsureContainerAsync()
    {
        var db = _cosmosClient.GetDatabase(_options.CosmosDbDatabase);
        await db.CreateContainerIfNotExistsAsync(
            id: _options.CosmosDbContainer,
            partitionKeyPath: "/auditId",
            cancellationToken: CancellationToken.None);
    }

    private async Task TryArmProvisionAsync()
    {
        try
        {
            // Best-effort ARM provisioning: Managed Identity RBAC for ARM may differ.
            // subscription id isn't listed in docs; if unavailable, skip ARM.
            var subscriptionId = Environment.GetEnvironmentVariable("AZURE_SUBSCRIPTION_ID");
            if (string.IsNullOrWhiteSpace(subscriptionId))
                return;

            ArmClient armClient = new ArmClient(_credential, subscriptionId);
            var accountResourceId = $"/subscriptions/{subscriptionId}/resourceGroups/{_options.ResourceGroupName}/providers/Microsoft.DocumentDB/databaseAccounts/{_options.AzureCosmosAccountName}";
            dynamic account = armClient.GetCosmosDBAccountResource(new Azure.Core.ResourceIdentifier(accountResourceId));

            // Database provisioning (best-effort)
            dynamic sqlDbCollection = account.GetCosmosDBSqlDatabaseCollection();
            var dbDataType = Type.GetType("Azure.ResourceManager.CosmosDB.Models.CosmosDBSqlDatabaseResourceData, Azure.ResourceManager.CosmosDB");
            var dbData = dbDataType is not null ? Activator.CreateInstance(dbDataType) : null;
            await sqlDbCollection.CreateOrUpdateAsync(
                waitUntil: Azure.WaitUntil.Completed,
                databaseName: _options.CosmosDbDatabase,
                data: dbData);

            // Container provisioning (best-effort)
            dynamic dbResource = account.GetCosmosDBSqlDatabaseResource(_options.CosmosDbDatabase);
            dynamic containerCollection = dbResource.GetCosmosDBSqlContainerCollection();
            var containerDataType = Type.GetType("Azure.ResourceManager.CosmosDB.Models.CosmosDBSqlContainerResourceData, Azure.ResourceManager.CosmosDB");
            var containerData = containerDataType is not null ? Activator.CreateInstance(containerDataType) : null;
            // If setting partition key fails due to type mismatches, data-plane provisioning below still guarantees container existence.
            try
            {
                var partitionKeyType = Type.GetType("Azure.ResourceManager.CosmosDB.Models.CosmosDBPartitionKey, Azure.ResourceManager.CosmosDB");
                var partitionKeyPathType = Type.GetType("Azure.ResourceManager.CosmosDB.Models.CosmosDBPartitionKeyPath, Azure.ResourceManager.CosmosDB");
                if (partitionKeyType is not null && partitionKeyPathType is not null && containerData is not null)
                {
                    var pkPath = Activator.CreateInstance(partitionKeyPathType, new object[] { "/auditId" });
                    var pk = Activator.CreateInstance(partitionKeyType, new object[] { new[] { pkPath } });
                    containerData.PartitionKey = pk;
                }
            }
            catch
            {
                // ignore and rely on data-plane provisioning
            }

            await containerCollection.CreateOrUpdateAsync(
                waitUntil: Azure.WaitUntil.Completed,
                containerName: _options.CosmosDbContainer,
                data: containerData);
        }
        catch
        {
            // Best-effort: ARM provisioning may fail due to RBAC.
        }
    }
}
