namespace SimulationOfDevices.Services.Common.Services
{
    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTime DateTimeUtcNow => DateTime.UtcNow;

        public DateTimeOffset DateTimeOffsetUtcNow => DateTimeOffset.UtcNow;
    }
}
