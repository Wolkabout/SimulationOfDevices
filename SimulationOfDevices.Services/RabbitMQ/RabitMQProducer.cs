using System.Text;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using Serilog;
using SimulationOfDevices.Services.Common.Services;
using SimulationOfDevices.Services.SettingsOptions;


namespace SimulationOfDevices.Services.RabbitMQ
{
    public interface IRabitMQProducer
    {        
        Task SendFeedValuesViaAmqp(Guid deviceId, DeviceSettings deviceSettings, CancellationToken cancellationToken);
    }

    public class RabitMQProducer : IRabitMQProducer
    {
        private readonly IRabbitMqConfigService _rabbitMqService;
        private readonly RabbitMQSettings _rabbitMQSettings;
        private readonly ILogger _logger;

        public RabitMQProducer(IRabbitMqConfigService rabbitMqService, IOptions<RabbitMQSettings> rabbitMQSettings, ILogger logger)
        {
            _logger = logger;
            _rabbitMqService = rabbitMqService;
            _rabbitMQSettings = rabbitMQSettings.Value;
        }

        public async Task SendFeedValuesViaAmqp(Guid deviceId, DeviceSettings deviceSettings, CancellationToken cancellationToken)
        {
            var feedSettings = CreateSettingsForSendFeed.Create(deviceSettings);

            try
            {                
                using PeriodicTimer timer = new PeriodicTimer(feedSettings.PublishRate);
                TimeSpan durationTimeForSendingData = feedSettings.PublishDuration;

                IConnection connection = _rabbitMqService.CreateConnection();
                using IModel channel = _rabbitMqService.CreateChannel(connection);                    
                var timeLeft = durationTimeForSendingData;

                while (!cancellationToken.IsCancellationRequested && await timer.WaitForNextTickAsync(cancellationToken))
                {
                    timeLeft = timeLeft.Subtract(feedSettings.PublishRate);
                    if (timeLeft <= TimeSpan.Zero)            
                    {
                        break;
                    }
                    try
                    {
                        if (feedSettings.Function.FunctionForSendingData == FunctionForSendingData.SKIP)
                        {
                            continue;
                        }

                        var messageForPlatform = CreateSettingsForSendFeed.CreateRandomValues(feedSettings.Reference, feedSettings.Function);

                        channel.ExchangeDeclare(exchange: _rabbitMQSettings.Exchange, type: _rabbitMQSettings.ExchangeType, durable: _rabbitMQSettings.Durable);
                        var json = JsonConvert.SerializeObject(messageForPlatform);
                        var body = Encoding.UTF8.GetBytes(json);

                        var fullTopic = $"d2p/{deviceId}/feed_values";

                        var basicProperties = channel.CreateBasicProperties();
                        basicProperties.Headers = new Dictionary<string, object>
                        {
                            { "mqtt_receivedTopic", fullTopic },
                            { "topic", fullTopic },
                            { "type", "d2p" }
                        };

                        _logger.Information("Message sent.");
                        channel.BasicPublish(exchange: _rabbitMQSettings.Exchange, routingKey: _rabbitMQSettings.RoutingKey, basicProperties: basicProperties, body: body);
                    }
                    catch (TaskCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Could not publish. Reason: {ex.Message}");
                        throw;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }              
    }    
}

