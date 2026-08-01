namespace OuterloopLabApi.Options;

public sealed class CosmosOptions
{
    public string CosmosDbUri { get; set; } = string.Empty;
    public string CosmosDbDatabase { get; set; } = string.Empty;
    public string CosmosDbContainer { get; set; } = string.Empty;
    public string AzureCosmosAccountName { get; set; } = string.Empty;
    public string ResourceGroupName { get; set; } = string.Empty;
    public string CosmosDbRegion { get; set; } = "Central India";
    public string AzureManagedIdentityClientId { get; set; } = string.Empty;

    public static CosmosOptions FromEnvironment()
    {
        static string GetRequired(string key)
        {
            var v = Environment.GetEnvironmentVariable(key);
            if (string.IsNullOrWhiteSpace(v))
                throw new InvalidOperationException($"Missing required environment variable: {key}");
            return v;
        }

        return new CosmosOptions
        {
            CosmosDbUri = GetRequired("COSMOS_DB_URI"),
            CosmosDbDatabase = GetRequired("COSMOS_DB_DATABASE"),
            CosmosDbContainer = GetRequired("COSMOS_DB_CONTAINER"),
            AzureCosmosAccountName = GetRequired("COSMOS_DB_ACCOUNT_NAME"),
            ResourceGroupName = GetRequired("COSMOS_DB_RESOURCE_GROUP"),
            CosmosDbRegion = Environment.GetEnvironmentVariable("COSMOS_DB_REGION") ?? "Central India",
            AzureManagedIdentityClientId = GetRequired("AZURE_MANAGED_IDENTITY_CLIENT_ID"),
        };
    }
}
