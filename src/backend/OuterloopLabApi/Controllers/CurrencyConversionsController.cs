using Microsoft.AspNetCore.Mvc;
using OuterloopLabApi.Models;
using OuterloopLabApi.Services;
using OuterloopLabApi.Services.Fx;

namespace OuterloopLabApi.Controllers;

[ApiController]
[Route("api/conversions")]
public sealed class CurrencyConversionsController : ControllerBase
{
    private readonly ConversionService _service;

    public CurrencyConversionsController(ConversionService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Convert([FromBody] ConversionRequest request, CancellationToken cancellationToken)
    {
        if (!ConversionRequestValidator.TryValidate(request, out var validationProblem))
            return validationProblem;

        try
        {
            var result = await _service.ConvertAsync(request, cancellationToken);
            return Ok(result);
        }
        catch (FxProviderUnavailableException)
        {
            return Problem(
                statusCode: StatusCodes.Status503ServiceUnavailable,
                title: "Currency conversion provider unavailable",
                detail: "Currency conversion provider unavailable",
                type: "https://example.com/problems/currency-conversion-provider-unavailable");
        }
        catch (FxQuoteNormalizationException)
        {
            return Problem(
                statusCode: StatusCodes.Status503ServiceUnavailable,
                title: "Currency conversion provider unavailable",
                detail: "Currency conversion provider unavailable",
                type: "https://example.com/problems/currency-conversion-provider-unavailable");
        }
    }

    [HttpGet("{auditId}")]
    public async Task<IActionResult> GetByAuditId([FromRoute] string auditId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(auditId))
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Invalid audit identifier", detail: "auditId is required.");

        var record = await _service.GetByAuditIdAsync(auditId, cancellationToken);
        if (record is null)
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Audit record not found",
                Detail = "No conversion audit record exists for the given auditId."
            });

        return Ok(record);
    }
}
