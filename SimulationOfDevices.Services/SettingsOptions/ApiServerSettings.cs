namespace SimulationOfDevices.Services.SettingsOptions
{
    public sealed class ApiServerSettings
    {
        public string ApiUrl { get; init; } = string.Empty;

        public string BearerToken { get; init; } = string.Empty;

        public string AesKey { get; init; } = string.Empty;
    }
}
