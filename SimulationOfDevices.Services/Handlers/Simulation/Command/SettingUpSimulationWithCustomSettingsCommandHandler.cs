using System.Dynamic;
using System.Text.Json;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SimulationOfDevices.DAL;
using SimulationOfDevices.Services.Common;
using SimulationOfDevices.Services.Common.Services;
using SimulationOfDevices.Services.Enums;
using SimulationOfDevices.Services.Models;
using SimulationOfDevices.Services.RabbitMQ;

namespace SimulationOfDevices.Services.Handlers.Simulation.Command
{
    public record SettingUpSimulationWithCustomSettingsCommand : IRequest<OperationResult<Unit>>
    {
        public Guid DeviceId { get; set; }
        public JsonElement Settings { get; set; }

        public SettingUpSimulationWithCustomSettingsCommand(Guid deviceId, JsonElement settings)
        {
            DeviceId = deviceId;
            Settings = settings;
        }

    }

    public class SettingUpSimulationWithCustomSettingsCommandHandler : IRequestHandler<SettingUpSimulationWithCustomSettingsCommand, OperationResult<Unit>>
    {
        private readonly OperationResult<Unit> _result = new();
        private readonly DataContext _dataContext;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IDateTimeProvider _dateTimeProvider;

        public SettingUpSimulationWithCustomSettingsCommandHandler(DataContext dataContext, IBackgroundJobClient backgroundJobClient, IDateTimeProvider dateTimeProvider)
        {
            _dataContext = dataContext;
            _backgroundJobClient = backgroundJobClient;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<OperationResult<Unit>> Handle(SettingUpSimulationWithCustomSettingsCommand request, CancellationToken cancellationToken)
        {
            using var transaction = await _dataContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var deviceGuid = request.DeviceId;
                var simulationDevice = await _dataContext.SimulationDevices.FirstOrDefaultAsync(x => x.DeviceGuid == deviceGuid, cancellationToken);

                if (simulationDevice is null)
                {
                    _result.AddError(ErrorCode.NotFound, string.Format(ErrorMessages.SimulationDeviceNotFound, simulationDevice));
                    return _result;
                }

                var mappedDictionary = MapSettingsToDictionary(request.Settings.ToString());

                if (mappedDictionary is null)
                {
                    _result.AddError(ErrorCode.FailedMappingToProperObject, ErrorMessages.FailedMappingToProperObject);
                    return _result;
                }
              
                //set setting for simulation device in simulationdevice table
                simulationDevice.Settings = request.Settings.ToString();

                //remove all previous jobs for that device
                var jobsToRemove = await _dataContext.SimulationJobs.Where(x => x.DeviceGuid == deviceGuid).ToListAsync();
                foreach (var job in jobsToRemove)
                {
                    var state = new DeletedState();
                    _backgroundJobClient.ChangeState(job.HangFireJobId.ToString(), state);
                }
                if (jobsToRemove.Any())
                {
                    _dataContext.SimulationJobs.RemoveRange(jobsToRemove);
                }                

                //iterating through setting and creating new jobs into simulationjob table
                foreach (var kpv in mappedDictionary)
                {                    
                    foreach (var jobParamater in kpv.Value.Select((item, index) => (item, index)))
                    {
                        var deviceSettings = new DeviceSettings()
                        {
                            Reference = kpv.Key,
                            JobParamaterModel = jobParamater.item
                        };
                       
                        var dateTimeNever = Helpers.DateThatNeverHappens();
                        TimeSpan durationOfTheJob = CreateSettingsForSendFeed.PublishRateOrDurationFromSettings(jobParamater.item.Duration);
                        
                        var jobCreatedId = _backgroundJobClient.Create<IRabitMQProducer>(x => x.SendFeedValuesViaAmqp(deviceGuid, deviceSettings, cancellationToken), new ScheduledState(dateTimeNever));

                        _dataContext.SimulationJobs.Add(new SimulationJob()
                        {
                            HangFireJobId = Convert.ToInt32(jobCreatedId),
                            DeviceGuid = deviceGuid,
                            CreatedDate = _dateTimeProvider.DateTimeUtcNow,
                            UpdatedDate = _dateTimeProvider.DateTimeUtcNow,
                            QueuePosition = jobParamater.index,
                            ReferenceKey = kpv.Key,
                            Duration = durationOfTheJob
                        });
                    }   
                }

                await _dataContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return _result;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }            
        }

        private Dictionary<string, List<JobParamatersModel>?>? MapSettingsToDictionary(string settings)
        {
            var mappedData = JsonConvert.DeserializeObject<ExpandoObject>(settings, new ExpandoObjectConverter());
            var dynamicDictionary = new Dictionary<string, object>(mappedData);
            Dictionary<string, List<JobParamatersModel>?>? mappedDictonary = new();
            //foreach (var item in dynamicDictionary)
            //{
            //    List<JobParamatersModel> jobParamaters = JsonConvert.DeserializeObject<List<JobParamatersModel>>(JsonConvert.SerializeObject(item.Value));
            //    mappedDictonary.Add(item.Key, jobParamaters);
            //}
            mappedDictonary = dynamicDictionary.ToDictionary(x => x.Key, x => JsonConvert.DeserializeObject<List<JobParamatersModel>>(JsonConvert.SerializeObject(x.Value)));
            return mappedDictonary;
        }
    }
}
