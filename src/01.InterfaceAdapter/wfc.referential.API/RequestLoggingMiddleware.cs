using System.Diagnostics;

namespace wfc.referential.API;
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
        var correlationId = context.TraceIdentifier;

        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["RequestPath"] = context.Request.Path,
            ["RequestMethod"] = context.Request.Method,
            ["UserAgent"] = context.Request.Headers["User-Agent"].ToString()
        }))
        {
            var stopwatch = Stopwatch.StartNew();

            _logger.LogInformation("HTTP Request started");

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "HTTP Request failed");
                throw;
            }
            finally
            {
                stopwatch.Stop();
                _logger.LogInformation("HTTP Request completed in {ElapsedMilliseconds}ms with status {StatusCode}",
                    stopwatch.ElapsedMilliseconds, context.Response.StatusCode);
            }
        }
    }
}
