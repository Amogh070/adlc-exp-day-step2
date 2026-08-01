namespace OuterloopLabApi.Models;

public sealed class FxQuote
{
    public decimal Rate { get; set; }
    public string ProviderDate { get; set; } = string.Empty;
}
