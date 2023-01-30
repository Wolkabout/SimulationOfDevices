using Microsoft.AspNetCore.Mvc.Filters;

namespace SimulationOfDevices.API.Filters
{
    public class ExceptionFilterHandler : IExceptionFilter
    {
        private readonly ILogger _logger;

        public ExceptionFilterHandler(ILogger logger)
        {
            _logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            _logger.Error(messageTemplate: context.Exception.Message);
            context.Result = new StatusCodeResult(500);
        }
    }
}
