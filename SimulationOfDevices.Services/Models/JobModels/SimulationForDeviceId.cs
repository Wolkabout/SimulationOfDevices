using Newtonsoft.Json;

namespace SimulationOfDevices.Services.Models.JobModels;

public sealed class SimulationForDeviceId
{
    // TODO: Add any other parameters for work
    [JsonProperty("DeviceGuid")]
    public Guid DeviceGuid { get; set; }
}
