namespace SimulationOfDevices.Services.Common.Services
{
    public interface IDateTimeProvider
    {
        DateTime DateTimeUtcNow { get; }

        DateTimeOffset DateTimeOffsetUtcNow { get; }
    }
}
