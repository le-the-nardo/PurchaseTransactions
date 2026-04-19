using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PurchaseTransactions.Api.Domain;

namespace PurchaseTransactions.Api.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        if (exception is OperationCanceledException)
            return true;

        ProblemDetails problem;

        if (exception is DomainException domainEx)
        {
            context.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
            problem = new ProblemDetails
            {
                Status = StatusCodes.Status422UnprocessableEntity,
                Title = "Validation Error",
                Detail = domainEx.Message,
                Extensions = { ["errorCode"] = domainEx.ErrorCode }
            };
        }
        else
        {
            _logger.LogError(exception, "Unhandled exception");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            problem = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An unexpected error occurred."
            };
        }

        await context.Response.WriteAsJsonAsync(problem, ct);
        return true;
    }
}
