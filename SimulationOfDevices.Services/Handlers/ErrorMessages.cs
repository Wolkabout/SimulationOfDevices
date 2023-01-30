namespace SimulationOfDevices.Services.Handlers
{
    public static class ErrorMessages
    {
        public const string NotFound = "Device with id: {0} in not found in database"; 
        public const string SomethingWenWrong = "Something went wrong!";
        public const string UnauthorizedAccessException = "Unauthorized call!";
        public const string JobNotFound = "Job for device with id {0} in not found in database";
        public const string SimulationDeviceNotFound = "Simulation device with id {0} in not found in database";
        public const string FailedMappingToProperObject = "Failed to map object properly";
    }
}
