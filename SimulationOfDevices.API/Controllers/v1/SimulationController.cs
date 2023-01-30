using System.Text.Json;
using ReadDataFromOPC;
using SimulationOfDevices.Services.Handlers.Simulation.Command;
using SimulationOfDevices.Services.Models.Dtos;
using SimulationOfDevices.Services.Models.JobModels;

namespace SimulationOfDevices.API.Controllers.v1;

[ApiController]
[Route("api/")]
[ApiVersion("1.0")]
public class SimulationController : BaseController
{
    public SimulationController() { }
    
    //[HttpGet("getDataFromApi")]
    //public async Task<IActionResult> GetDataFromApi(string restOfTheRoute, CancellationToken cancellationToken)
    //{
    //    var query = new GetDataFromApiQuery(restOfTheRoute);
    //    var result = await _mediator.Send(query, cancellationToken);

    //    if (result.IsError) return HandleErrorResponse(result.Errors);
    //    return Ok(result);
    //}

    //[HttpGet("SendDataToPlatform")]
    //public async Task<IActionResult> SendDataToPlatfom()
    //{

    //    //var command = new SendDataToPlatfomViaAmqp(createModel);
    //    //var result = await _mediator.Send(command);

    //    //return result.ResponseStatus == ResponseStatus.Error ? StatusCode(StatusCodes.Status500InternalServerError) : Ok(result);
    //    await foreach (var item in DataFromOpc.ReadDataFromOpc())
    //    {
    //        try
    //        {
    //            _logger.Information($"Sending message: {item}");
    //            Console.WriteLine(item);
    //        }
    //        catch (Exception ex)
    //        {
    //            return Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
    //        }
    //    }

    //    return Ok();
    //}

    //[HttpGet, Route("getAllJobs")]
    //public IActionResult GetAllJobsAsync()
    //{
    //    //    var monitoringApi = JobStorage.Current.GetMonitoringApi();
    //    //    return Ok(monitoringApi.("default", 1, 100));
    //    //return Ok(await _simulationJobStatusService.GetAllJobsAsync().ConfigureAwait(false));
    //    //List<RecurringJobDto> recurringJobs = new List<RecurringJobDto>();


    //    //var recurringJobs = JobStorage.Current.GetConnection().GetRecurringJobs();
    //    List<string> listOfJobsIds = new();
    //    //listOfJobsIds.AddRange(recurringJobs.Select(item => item.Id));


    //    var runningJobs = JobStorage.Current.GetMonitoringApi().ProcessingJobs(0, 100);
    //    listOfJobsIds.AddRange(runningJobs.Select(x => x.Key));
    //    var json = JsonConvert.SerializeObject(runningJobs, Formatting.Indented, new JsonSerializerSettings
    //    {
    //        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
    //    });

    //    return Ok(json);
    //}

    [HttpPost("simulatedDevices")]
    public async Task<IActionResult> SimulatedDevices([FromBody] SimulationForDeviceId model, CancellationToken cancellationToken)
    {
        _logger.Information($"Setting up simultaion for device with id: {model.DeviceGuid}");
        var command = new SettingUpSimulationCommand(model);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsError) return HandleErrorResponse(result.Errors);
        return Ok(result);
    }

    [HttpPut("simulatedDevices/{deviceId}/settings")]
    public async Task<IActionResult> SimulatedDevicesWithSettings(Guid deviceId, [FromBody] JsonElement settings, CancellationToken cancellationToken)
    {
        _logger.Information($"Setting up simulation with additinal settings for device with id: {deviceId}");
        var command = new SettingUpSimulationWithCustomSettingsCommand(deviceId, settings);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsError) return HandleErrorResponse(result.Errors);
        return Ok(result);
    }

    [HttpPost("simulatedDevices/{deviceId}/start")]
    public async Task<IActionResult> StartSimulation(Guid deviceId, CancellationToken cancellationToken)
    {
        _logger.Information($"Starting simulation for device with id: {deviceId}");
        var command = new StartingSimulationCommand(deviceId);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsError) return HandleErrorResponse(result.Errors);
        return Ok(result);
    }

    [HttpPost("simulatedDevices/{deviceId}/stop")]
    public async Task<IActionResult> StopSimulation(Guid deviceId, CancellationToken cancellationToken)
    {
        _logger.Information($"Stoping simulation for device with id: {deviceId}");
        var command = new StoppingSimulationCommand(deviceId);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsError) return HandleErrorResponse(result.Errors);
        return Ok(result);
    }
}
