namespace SimulationOfDevices.API.Middleware;

public class OperationCanceledMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger _logger;
    public OperationCanceledMiddleware(RequestDelegate next, ILogger logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (OperationCanceledException)
        {
            _logger.Information("Request was cancelled");
            context.Response.StatusCode = 409;
        }
    }
}
