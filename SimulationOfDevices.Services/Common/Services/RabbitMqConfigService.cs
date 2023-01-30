using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using SimulationOfDevices.Services.SettingsOptions;

namespace SimulationOfDevices.Services.Common.Services
{
    public class RabbitMqConfigService : IRabbitMqConfigService
    {
        private readonly RabbitMQSettings _rabbitMQSettings;
        private readonly ApiServerSettings _apiServerSettings;

        public RabbitMqConfigService(IOptionsMonitor<RabbitMQSettings> options, IOptionsMonitor<ApiServerSettings> apiServerSettings)
        {
            _rabbitMQSettings = options.CurrentValue;
            _apiServerSettings = apiServerSettings.CurrentValue;
        }

        public IConnection CreateConnection()
        {
            var decryptedUserName = Helpers.DecryptString(_apiServerSettings.AesKey, _rabbitMQSettings.UserName);
            var decryptedPassword = Helpers.DecryptString(_apiServerSettings.AesKey, _rabbitMQSettings.Password);

            ConnectionFactory connection = new ConnectionFactory()
            {
                HostName = _rabbitMQSettings.Host,
                Port = _rabbitMQSettings.Port,
                VirtualHost = _rabbitMQSettings.VHost,
                UserName = decryptedUserName,
                Password = decryptedPassword,
                DispatchConsumersAsync = true
            };

            return connection.CreateConnection();
        }

        public IModel CreateChannel(IConnection connection)
        {
            var channel = connection.CreateModel();
            return channel;
        }
    }
}
