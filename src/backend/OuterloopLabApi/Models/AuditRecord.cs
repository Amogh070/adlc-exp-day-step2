using System.Text.Json.Serialization;

namespace OuterloopLabApi.Models;

public sealed class AuditRecord
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("auditId")]
    public string AuditId { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    [JsonPropertyName("fromCurrency")]
    public string FromCurrency { get; set; } = string.Empty;

    [JsonPropertyName("toCurrency")]
    public string ToCurrency { get; set; } = string.Empty;

    [JsonPropertyName("rate")]
    public decimal Rate { get; set; }

    [JsonPropertyName("convertedAmount")]
    public decimal ConvertedAmount { get; set; }

    [JsonPropertyName("providerDate")]
    public string ProviderDate { get; set; } = string.Empty;

    [JsonPropertyName("executedAtUtc")]
    public DateTimeOffset ExecutedAtUtc { get; set; }
}
