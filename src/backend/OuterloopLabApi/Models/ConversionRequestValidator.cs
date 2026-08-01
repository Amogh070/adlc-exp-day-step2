using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;

namespace OuterloopLabApi.Models;

public static class ConversionRequestValidator
{
    private static readonly Regex CurrencyCodeRegex = new("^[A-Z]{3}$", RegexOptions.Compiled);

    public static bool TryValidate(ConversionRequest? request, out IActionResult? problem)
    {
        problem = null;
        if (request is null)
        {
            problem = new BadRequestObjectResult(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid request",
                Detail = "Request body is required."
            });
            return false;
        }

        if (request.Amount <= 0)
        {
            problem = new BadRequestObjectResult(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid amount",
                Detail = "amount must be greater than 0"
            });
            return false;
        }

        var from = request.FromCurrency?.Trim();
        if (string.IsNullOrWhiteSpace(from) || !CurrencyCodeRegex.IsMatch(from))
        {
            problem = new BadRequestObjectResult(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid fromCurrency",
                Detail = "fromCurrency must be a three-letter uppercase ISO currency code"
            });
            return false;
        }

        var to = request.ToCurrency?.Trim();
        if (string.IsNullOrWhiteSpace(to) || !CurrencyCodeRegex.IsMatch(to))
        {
            problem = new BadRequestObjectResult(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid toCurrency",
                Detail = "toCurrency must be a three-letter uppercase ISO currency code"
            });
            return false;
        }

        request.FromCurrency = from.ToUpperInvariant();
        request.ToCurrency = to.ToUpperInvariant();
        return true;
    }
}
