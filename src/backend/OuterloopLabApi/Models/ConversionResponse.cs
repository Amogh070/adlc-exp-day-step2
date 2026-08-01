namespace OuterloopLabApi.Models;

public sealed class ConversionResponse
{
    public string AuditId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string FromCurrency { get; set; } = string.Empty;
    public string ToCurrency { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public decimal ConvertedAmount { get; set; }
    public string ProviderDate { get; set; } = string.Empty;
    public DateTimeOffset ExecutedAtUtc { get; set; }
}
