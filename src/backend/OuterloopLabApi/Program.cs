using Azure.Identity;
using Microsoft.Azure.Cosmos;
using Microsoft.OpenApi.Models;
using OuterloopLabApi.Options;
using OuterloopLabApi.Services;
using OuterloopLabApi.Services.Cosmos;
using OuterloopLabApi.Services.Fx;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.SwaggerDoc("v1", new OpenApiInfo { Title = "OuterloopLabApi", Version = "v1" });
});

var cosmosOptions = CosmosOptions.FromEnvironment();
builder.Services.AddSingleton(cosmosOptions);
builder.Services.AddSingleton(FxOptions.FromEnvironment());

builder.Services.AddSingleton(sp =>
{
    var opts = new DefaultAzureCredentialOptions
    {
        ManagedIdentityClientId = cosmosOptions.AzureManagedIdentityClientId
    };
    return new DefaultAzureCredential(opts);
});

builder.Services.AddSingleton<CosmosClientFactory>();
builder.Services.AddSingleton(sp =>
{
    var options = sp.GetRequiredService<CosmosOptions>();
    var credential = sp.GetRequiredService<DefaultAzureCredential>();
    var factory = sp.GetRequiredService<CosmosClientFactory>();
    return factory.Create(options, credential);
});

builder.Services.AddSingleton<IAuditRepository, CosmosAuditRepository>();
builder.Services.AddSingleton<ConversionService>();
builder.Services.AddSingleton<CosmosInitializer>();

builder.Services.AddHttpClient<FrankfurterFxQuoteProvider>((sp, client) =>
{
    var options = sp.GetRequiredService<FxOptions>();
    client.BaseAddress = new Uri(options.CurrencyApiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(10);
});

builder.Services.AddSingleton<IFxQuoteProvider>(sp => sp.GetRequiredService<FrankfurterFxQuoteProvider>());

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<CosmosInitializer>();
    await initializer.InitializeAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
