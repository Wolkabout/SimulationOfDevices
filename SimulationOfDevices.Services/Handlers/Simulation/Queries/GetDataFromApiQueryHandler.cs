using System.Dynamic;
using MediatR;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Serilog;
using SimulationOfDevices.Services.BaseApiService;
using SimulationOfDevices.Services.Common;
using SimulationOfDevices.Services.Enums;
using SimulationOfDevices.Services.Models;
using SimulationOfDevices.Services.SettingsOptions;

namespace SimulationOfDevices.Services.Handlers.Simulation.Queries
{
    public record GetDataFromApiQuery : IRequest<OperationResult<List<ExpandoObject>>>
    {
        public string RestOfTheRoute { get; set; }

        public GetDataFromApiQuery(string restOfTheRoute)
        {
            RestOfTheRoute = restOfTheRoute;
        }
    }

    public sealed class GetDataFromApiQueryHandler : IRequestHandler<GetDataFromApiQuery, OperationResult<List<ExpandoObject>>>
    {

        private readonly IApiService _apiService;
        private readonly ApiServerSettings _apiServerSettings;
        private readonly ILogger _logger;
        public GetDataFromApiQueryHandler(IApiService apiService, IOptionsMonitor<ApiServerSettings> apiServerSettings, ILogger logger)
        {
            _logger = logger;
            _apiService = apiService;
            _apiServerSettings = apiServerSettings.CurrentValue;
        }


        public async Task<OperationResult<List<ExpandoObject>>> Handle(GetDataFromApiQuery request, CancellationToken cancellationToken)
        {
            var response = new OperationResult<List<ExpandoObject>>();
            var restOfTheRoute = request.RestOfTheRoute;

            string apiUrl = $"{_apiServerSettings.ApiUrl}/{restOfTheRoute}";
            string bearerToken = _apiServerSettings.BearerToken;

            try
            {
                var dataFromApi = await _apiService.GetDataFromApi(apiUrl, bearerToken);
                if (dataFromApi.IsError)
                {
                    dataFromApi.Errors.ForEach(x=> response.AddError(x.Code, x.Message));
                    return response;
                }

                if (dataFromApi.Payload.IsResponseArray())
                {
                    var mappedDataArray = JsonConvert.DeserializeObject<List<ExpandoObject>>(dataFromApi.Payload, new ExpandoObjectConverter());
                    response.Payload = mappedDataArray;
                    return response;
                }

                var mappedData = JsonConvert.DeserializeObject<ExpandoObject>(dataFromApi.Payload, new ExpandoObjectConverter());

                var responseData = new List<ExpandoObject>
                {
                    mappedData
                };

                response.Payload = responseData;
            }            
            catch (Exception ex)
            {
                _logger.Error($"Getting data failed! {ex.Message}");
                response.AddError(ErrorCode.ServerError, ErrorMessages.SomethingWenWrong);
            }

            return response;
        }
    }
}
