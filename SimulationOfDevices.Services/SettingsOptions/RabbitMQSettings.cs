namespace SimulationOfDevices.Services.SettingsOptions
{
    public sealed class RabbitMQSettings
    {
        public string Host { get; init; } = string.Empty;
        public int Port { get; init; }
        public string VHost { get; init; } = string.Empty;
        public string Exchange { get; init; } = string.Empty;
        public string ExchangeType { get; init; } = string.Empty;
        public bool Durable { get; init; }
        public string RoutingKey { get; init; } = string.Empty;                
        public string UserName { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
    }
}
