namespace OuterloopLabApi.Options;

public sealed class FxOptions
{
    // Frankfurter's public API lives at api.frankfurter.dev.
    public string CurrencyApiBaseUrl { get; set; } = "https://api.frankfurter.dev";

    public static FxOptions FromEnvironment()
    {
        return new FxOptions
        {
            CurrencyApiBaseUrl = Environment.GetEnvironmentVariable("CURRENCY_API_BASE_URL")
                                   ?? "https://api.frankfurter.dev"
        };
    }
}
