namespace SimulationOfDevices.Services.Common.Authentication
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(string deviceId, string firstName, string lastName);
    }
}
