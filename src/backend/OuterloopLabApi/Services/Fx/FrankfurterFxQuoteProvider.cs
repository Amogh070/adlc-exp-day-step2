using System.Text.Json;
using OuterloopLabApi.Models;
using OuterloopLabApi.Options;

namespace OuterloopLabApi.Services.Fx;

public sealed class FrankfurterFxQuoteProvider : IFxQuoteProvider
{
    private readonly HttpClient _httpClient;
    private readonly FxOptions _options;

    public FrankfurterFxQuoteProvider(HttpClient httpClient, FxOptions options)
    {
        _httpClient = httpClient;
        _options = options;
    }

    public async Task<FxQuote> GetQuoteAsync(string fromCurrency, string toCurrency, CancellationToken cancellationToken)
    {
        try
        {
            // Generic base URL; Frankfurter-like providers expose `date` + `rates`.
            // We avoid rigid response schema assumptions by normalizing from JSON.
            var requestUri = $"/v1/latest?from={Uri.EscapeDataString(fromCurrency)}&to={Uri.EscapeDataString(toCurrency)}";
            using var response = await _httpClient.GetAsync(requestUri, cancellationToken);
            if (!response.IsSuccessStatusCode)
                throw new FxProviderUnavailableException();

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
            var root = doc.RootElement;

            return FxQuoteNormalizer.Normalize(root, fromCurrency, toCurrency);
        }
        catch (FxProviderUnavailableException)
        {
            throw;
        }
        catch (FxQuoteNormalizationException)
        {
            throw;
        }
        catch
        {
            throw new FxProviderUnavailableException();
        }
    }
}
