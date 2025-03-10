namespace DDD.API.Middlewares;

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
        _logger.LogInformation(
            "HTTP {RequestMethod} {RequestPath} started",
            context.Request.Method,
            context.Request.Path);
            
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            await _next(context);
            stopwatch.Stop();
            
            _logger.LogInformation(
                "HTTP {RequestMethod} {RequestPath} completed in {ElapsedMilliseconds}ms with status code {StatusCode}",
                context.Request.Method,
                context.Request.Path,
                stopwatch.ElapsedMilliseconds,
                context.Response.StatusCode);
        }
        catch (Exception)
        {
            stopwatch.Stop();
            _logger.LogWarning(
                "HTTP {RequestMethod} {RequestPath} failed after {ElapsedMilliseconds}ms",
                context.Request.Method,
                context.Request.Path,
                stopwatch.ElapsedMilliseconds);
                
            throw;  // 例外は次のミドルウェアで処理するために再スロー
        }
    }
}
