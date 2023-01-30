namespace SimulationOfDevices.Services.Enums;

public enum ErrorCode
{
    NotFound = 404,
    ServerError = 500,

    //Validation errors should be in the range 100 - 199
    ValidationError = 101,
    

    //Infrastructure errors should be in the range 200-299    
    DatabaseOperationException = 203,

    //Application errors should be in the range 300 - 399
    FailedMappingToProperObject = 300,


    Unauthorized = 401,

    UnknownError = 999


}