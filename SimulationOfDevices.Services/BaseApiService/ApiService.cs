using System.Net;
using System.Net.Http.Headers;
using SimulationOfDevices.Services.Enums;
using SimulationOfDevices.Services.Handlers;
using SimulationOfDevices.Services.Models;

namespace SimulationOfDevices.Services.BaseApiService
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _client;

        public ApiService(HttpClient client)
        {
            _client = client;
        }

        public async Task<OperationResult<string>> GetDataFromApi(string apiUrl, string bearerToken)
        {

            var result = new OperationResult<string>();
            try
            {
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

                var response = await _client.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    result.Payload = content;                
                }
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    result.AddError(ErrorCode.Unauthorized, ErrorMessages.UnauthorizedAccessException);
                }            
            }
            catch (Exception)
            {                
                throw;
            }
            return result;
        }      
    }

}
