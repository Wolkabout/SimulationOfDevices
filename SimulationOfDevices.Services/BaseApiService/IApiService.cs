using SimulationOfDevices.Services.Models;

namespace SimulationOfDevices.Services.BaseApiService
{
    public interface IApiService
    {
        Task<OperationResult<string>> GetDataFromApi(string apiUrl, string bearerToken);        
    }
}