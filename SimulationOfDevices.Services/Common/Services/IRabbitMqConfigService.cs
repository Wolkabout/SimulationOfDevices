using RabbitMQ.Client;

namespace SimulationOfDevices.Services.Common.Services
{
    public interface IRabbitMqConfigService
    {
        IConnection CreateConnection();

        IModel CreateChannel(IConnection connection);
    }
}
