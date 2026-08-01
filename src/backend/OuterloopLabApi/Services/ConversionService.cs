using OuterloopLabApi.Models;
using OuterloopLabApi.Services.Cosmos;
using OuterloopLabApi.Services.Fx;

namespace OuterloopLabApi.Services;

public sealed class ConversionService
{
    private readonly IFxQuoteProvider _fxProvider;
    private readonly IAuditRepository _auditRepository;

    public ConversionService(IFxQuoteProvider fxProvider, IAuditRepository auditRepository)
    {
        _fxProvider = fxProvider;
        _auditRepository = auditRepository;
    }

    public async Task<ConversionResponse> ConvertAsync(ConversionRequest request, CancellationToken cancellationToken)
    {
        var quote = await _fxProvider.GetQuoteAsync(request.FromCurrency, request.ToCurrency, cancellationToken);
        var executedAtUtc = DateTimeOffset.UtcNow;

        var rate = DecimalRound(quote.Rate, 4);
        var convertedAmount = DecimalRound(request.Amount * rate, 2);

        var auditId = Guid.NewGuid().ToString("N");
        var record = new AuditRecord
        {
            Id = auditId,
            AuditId = auditId,
            Amount = request.Amount,
            FromCurrency = request.FromCurrency,
            ToCurrency = request.ToCurrency,
            Rate = rate,
            ConvertedAmount = convertedAmount,
            ProviderDate = quote.ProviderDate,
            ExecutedAtUtc = executedAtUtc
        };

        await _auditRepository.CreateAsync(record, cancellationToken);

        return new ConversionResponse
        {
            AuditId = auditId,
            Amount = request.Amount,
            FromCurrency = request.FromCurrency,
            ToCurrency = request.ToCurrency,
            Rate = rate,
            ConvertedAmount = convertedAmount,
            ProviderDate = quote.ProviderDate,
            ExecutedAtUtc = executedAtUtc
        };
    }

    public async Task<ConversionResponse?> GetByAuditIdAsync(string auditId, CancellationToken cancellationToken)
    {
        var record = await _auditRepository.GetByAuditIdAsync(auditId, cancellationToken);
        if (record is null)
            return null;

        return new ConversionResponse
        {
            AuditId = record.AuditId,
            Amount = record.Amount,
            FromCurrency = record.FromCurrency,
            ToCurrency = record.ToCurrency,
            Rate = record.Rate,
            ConvertedAmount = record.ConvertedAmount,
            ProviderDate = record.ProviderDate,
            ExecutedAtUtc = record.ExecutedAtUtc
        };
    }

    private static decimal DecimalRound(decimal value, int decimals)
        => Math.Round(value, decimals, MidpointRounding.AwayFromZero);
}
