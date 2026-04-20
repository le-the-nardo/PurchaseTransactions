using System.Diagnostics;

namespace PurchaseTransactions.Api.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault()
                            ?? context.TraceIdentifier;

        context.Response.Headers["X-Correlation-ID"] = correlationId;
        context.Items["CorrelationId"] = correlationId;

        var sw = Stopwatch.StartNew();

        _logger.LogInformation("[RequestLoggingMiddleware] Request started {Method} {Path} | correlationId={CorrelationId}",
            context.Request.Method, context.Request.Path, correlationId);

        try
        {
            await _next(context);
        }
        finally
        {
            sw.Stop();
            var level = context.Response.StatusCode >= 500 ? LogLevel.Error
                      : context.Response.StatusCode >= 400 ? LogLevel.Warning
                      : LogLevel.Information;

            _logger.Log(level,
                "[RequestLoggingMiddleware] Request completed {Method} {Path} → {StatusCode} in {ElapsedMs}ms | correlationId={CorrelationId}",
                context.Request.Method, context.Request.Path,
                context.Response.StatusCode, sw.ElapsedMilliseconds, correlationId);
        }
    }
}
