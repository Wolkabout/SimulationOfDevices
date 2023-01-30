using MediatR;
using Serilog;
using SimulationOfDevices.Services.Models;

namespace SimulationOfDevices.Services.Behaviors
{
    public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
        where TResponse : OperationResult<TResponse>
    {
        private readonly ILogger _logger;

        public LoggingBehavior(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            _logger.Information("Starting request {@RequestName}, {@DateTimeUtc}", typeof(TRequest).Name, DateTime.UtcNow);

            var result = await next();

            if (result.IsError)
            {
                _logger.Error("Request failure {@RequestName},{@Error} ,{@DateTimeUtc}", 
                    typeof(TRequest).Name, 
                    result.Errors,
                    DateTime.UtcNow);
            }

            _logger.Information("Completed request {@RequestName}, {@DateTimeUtc}", typeof(TRequest).Name, DateTime.UtcNow);

            return result;
        }
    }
}
