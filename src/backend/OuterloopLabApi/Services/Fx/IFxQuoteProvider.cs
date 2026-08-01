using OuterloopLabApi.Models;

namespace OuterloopLabApi.Services.Fx;

public interface IFxQuoteProvider
{
    Task<FxQuote> GetQuoteAsync(string fromCurrency, string toCurrency, CancellationToken cancellationToken);
}
