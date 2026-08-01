using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using OuterloopLabApi.Controllers;
using OuterloopLabApi.Models;
using OuterloopLabApi.Services;
using OuterloopLabApi.Services.Cosmos;
using OuterloopLabApi.Services.Fx;

namespace Tests;

public sealed class ConversionServiceTests
{
    [Fact]
    public async Task Convert_Success_PersistsAndReturnsExecutedAtUtcExactly()
    {
        var quote = new FxQuote { Rate = 0.92004m, ProviderDate = "2026-08-01" };
        var provider = new FakeFxQuoteProvider(quote);
        var repo = new InMemoryAuditRepository();
        var service = new ConversionService(provider, repo);

        var request = new ConversionRequest { Amount = 100.00m, FromCurrency = "USD", ToCurrency = "EUR" };

        var response = await service.ConvertAsync(request, CancellationToken.None);

        response.AuditId.Should().NotBeNullOrWhiteSpace();
        response.Rate.Should().Be(0.9200m);
        response.ConvertedAmount.Should().Be(92.00m);
        response.ProviderDate.Should().Be("2026-08-01");

        var stored = await repo.GetByAuditIdAsync(response.AuditId, CancellationToken.None);
        stored.Should().NotBeNull();
        stored!.ExecutedAtUtc.Should().Be(response.ExecutedAtUtc);
        stored.ConvertedAmount.Should().Be(92.00m);
    }

    [Fact]
    public async Task GetByAuditId_ReturnsPersistedRecord()
    {
        var quote = new FxQuote { Rate = 1.23456m, ProviderDate = "2026-08-01" };
        var provider = new FakeFxQuoteProvider(quote);
        var repo = new InMemoryAuditRepository();
        var service = new ConversionService(provider, repo);

        var request = new ConversionRequest { Amount = 50m, FromCurrency = "USD", ToCurrency = "CAD" };
        var response = await service.ConvertAsync(request, CancellationToken.None);

        var loaded = await service.GetByAuditIdAsync(response.AuditId, CancellationToken.None);
        loaded.Should().NotBeNull();
        loaded!.AuditId.Should().Be(response.AuditId);
        loaded.ExecutedAtUtc.Should().Be(response.ExecutedAtUtc);
        loaded.ConvertedAmount.Should().Be(response.ConvertedAmount);
    }

    [Fact]
    public async Task Controller_Returns400_ForInvalidCurrencyCode()
    {
        var provider = new FakeFxQuoteProvider(new FxQuote { Rate = 1m, ProviderDate = "2026-08-01" });
        var repo = new InMemoryAuditRepository();
        var service = new ConversionService(provider, repo);
        var controller = new CurrencyConversionsController(service);

        var badRequest = new ConversionRequest { Amount = 10m, FromCurrency = "US", ToCurrency = "EUR" };
        var result = await controller.Convert(badRequest, CancellationToken.None);

        var bad = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        bad.StatusCode.Should().Be(400);
        var details = bad.Value.Should().BeAssignableTo<ProblemDetails>().Subject;
        details.Title.Should().Be("Invalid fromCurrency");
    }

    [Fact]
    public async Task Controller_Returns503_AndDoesNotPersist_WhenProviderUnavailable()
    {
        var provider = new ThrowingFxQuoteProvider(new FxProviderUnavailableException());
        var repo = new InMemoryAuditRepository();
        var service = new ConversionService(provider, repo);
        var controller = new CurrencyConversionsController(service);

        var request = new ConversionRequest { Amount = 10m, FromCurrency = "USD", ToCurrency = "EUR" };
        var result = await controller.Convert(request, CancellationToken.None);

        var problem = result.Should().BeOfType<ObjectResult>().Subject;
        problem.StatusCode.Should().Be(503);
        var details = problem.Value.Should().BeAssignableTo<ProblemDetails>().Subject;
        details.Title.Should().Be("Currency conversion provider unavailable");
        repo.Count.Should().Be(0);
    }

    [Fact]
    public void FxQuoteNormalizer_SupportsRatesAndConversionRates()
    {
        var json1 = "{\"date\":\"2026-08-01\",\"rates\":{\"EUR\":0.87}}";
        using var doc1 = JsonDocument.Parse(json1);
        var q1 = FxQuoteNormalizer.Normalize(doc1.RootElement, "USD", "EUR");
        q1.ProviderDate.Should().Be("2026-08-01");
        q1.Rate.Should().Be(0.87m);

        var json2 = "{\"date\":\"2026-08-01\",\"conversion_rates\":{\"EUR\":0.88}}";
        using var doc2 = JsonDocument.Parse(json2);
        var q2 = FxQuoteNormalizer.Normalize(doc2.RootElement, "USD", "EUR");
        q2.Rate.Should().Be(0.88m);
    }

    private sealed class FakeFxQuoteProvider : IFxQuoteProvider
    {
        private readonly FxQuote _quote;
        public FakeFxQuoteProvider(FxQuote quote) => _quote = quote;
        public Task<FxQuote> GetQuoteAsync(string fromCurrency, string toCurrency, CancellationToken cancellationToken)
            => Task.FromResult(_quote);
    }

    private sealed class ThrowingFxQuoteProvider : IFxQuoteProvider
    {
        private readonly Exception _ex;
        public ThrowingFxQuoteProvider(Exception ex) => _ex = ex;
        public Task<FxQuote> GetQuoteAsync(string fromCurrency, string toCurrency, CancellationToken cancellationToken)
            => Task.FromException<FxQuote>(_ex);
    }

    private sealed class InMemoryAuditRepository : IAuditRepository
    {
        private readonly Dictionary<string, AuditRecord> _data = new();
        public int Count => _data.Count;

        public Task CreateAsync(AuditRecord record, CancellationToken cancellationToken)
        {
            _data[record.AuditId] = record;
            return Task.CompletedTask;
        }

        public Task<AuditRecord?> GetByAuditIdAsync(string auditId, CancellationToken cancellationToken)
        {
            _data.TryGetValue(auditId, out var record);
            return Task.FromResult(record);
        }
    }
}
