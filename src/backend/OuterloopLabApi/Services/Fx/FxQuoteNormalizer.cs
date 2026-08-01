using System.Text.Json;
using OuterloopLabApi.Models;

namespace OuterloopLabApi.Services.Fx;

public static class FxQuoteNormalizer
{
    public static FxQuote Normalize(JsonElement root, string fromCurrency, string toCurrency)
    {
        if (!root.TryGetProperty("date", out var dateElement) || dateElement.ValueKind != JsonValueKind.String)
            throw new FxQuoteNormalizationException();

        var providerDate = dateElement.GetString();
        if (string.IsNullOrWhiteSpace(providerDate))
            throw new FxQuoteNormalizationException();

        // Some providers return a single rate at the top-level (e.g. `{ rate: ..., date: ... }`).
        if (root.TryGetProperty("rate", out var topRateElement) &&
            (topRateElement.ValueKind == JsonValueKind.Number || topRateElement.ValueKind == JsonValueKind.String))
        {
            var rate = ParseDecimal(topRateElement);
            if (rate > 0)
                return new FxQuote { Rate = rate, ProviderDate = providerDate };
        }

        // Others return a map of rates.
        JsonElement ratesContainer;
        if (root.TryGetProperty("rates", out var rates) && rates.ValueKind == JsonValueKind.Object)
            ratesContainer = rates;
        else if (root.TryGetProperty("conversion_rates", out var conversionRates) && conversionRates.ValueKind == JsonValueKind.Object)
            ratesContainer = conversionRates;
        else
            throw new FxQuoteNormalizationException();

        if (!ratesContainer.TryGetProperty(toCurrency, out var rateElement) ||
            (rateElement.ValueKind != JsonValueKind.Number && rateElement.ValueKind != JsonValueKind.String))
            throw new FxQuoteNormalizationException();

        var parsedRate = ParseDecimal(rateElement);
        if (parsedRate <= 0)
            throw new FxQuoteNormalizationException();

        return new FxQuote
        {
            Rate = parsedRate,
            ProviderDate = providerDate
        };
    }

    private static decimal ParseDecimal(JsonElement element)
    {
        return element.ValueKind == JsonValueKind.Number
            ? element.GetDecimal()
            : decimal.Parse(element.GetString() ?? "");
    }
}
