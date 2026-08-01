namespace OuterloopLabApi.Services.Fx;

public sealed class FxProviderUnavailableException : Exception
{
    public FxProviderUnavailableException() : base("Currency provider unavailable") { }
}

public sealed class FxQuoteNormalizationException : Exception
{
    public FxQuoteNormalizationException() : base("Currency provider response could not be normalized") { }
}
