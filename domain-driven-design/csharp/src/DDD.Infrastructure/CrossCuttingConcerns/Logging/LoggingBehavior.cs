namespace DDD.Infrastructure.CrossCuttingConcerns.Logging;

using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

public class LoggingBehavior<TRequest, TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
    private readonly Func<TRequest, CancellationToken, Task<TResponse>> _next;
    
    public LoggingBehavior(
        ILogger<LoggingBehavior<TRequest, TResponse>> logger,
        Func<TRequest, CancellationToken, Task<TResponse>> next)
    {
        _logger = logger;
        _next = next;
    }
    
    public async Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling {RequestType}", typeof(TRequest).Name);
        
        try
        {
            var response = await _next(request, cancellationToken);
            _logger.LogInformation("Handled {RequestType}", typeof(TRequest).Name);
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling {RequestType}", typeof(TRequest).Name);
            throw;
        }
    }
}