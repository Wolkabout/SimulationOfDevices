using SimulationOfDevices.Services.Enums;

namespace SimulationOfDevices.Services.Models;

public class Error
{
    public ErrorCode Code { get; set; }
    public string Message { get; set; }
}