using SimulationOfDevices.Services.Models;

namespace SimulationOfDevices.Services.RabbitMQ
{
    public class DeviceSettings
    {
        public string Reference { get; set; }
        public JobParamatersModel JobParamaterModel { get; set; }
    }

    public class FeedSettings
    {
        public string Reference { get; set; }
        public TimeSpan PublishRate { get; set; }
        public TimeSpan PublishDuration { get; set; }
        public FunctionFromSettings Function { get; set; }
    }
}
