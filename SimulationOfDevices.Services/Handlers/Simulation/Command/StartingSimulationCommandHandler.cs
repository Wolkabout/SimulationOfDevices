using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SimulationOfDevices.DAL;
using SimulationOfDevices.Services.Common;
using SimulationOfDevices.Services.Common.Services;
using SimulationOfDevices.Services.Enums;
using SimulationOfDevices.Services.Models;
using SimulationOfDevices.Services.RabbitMQ;

namespace SimulationOfDevices.Services.Handlers.Simulation.Command
{
    public record StartingSimulationCommand : IRequest<OperationResult<Unit>>
    {
        public Guid DeviceGuid { get; set; }

        public StartingSimulationCommand(Guid deviceGuid)
        {
            DeviceGuid = deviceGuid;
        }
    }

    public sealed class StartingSimulationCommandHandler : IRequestHandler<StartingSimulationCommand, OperationResult<Unit>>
    {
        private readonly OperationResult<Unit> _result = new();
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly DataContext _dataContext;
        private readonly IDateTimeProvider _dateTimeProvider;

        public StartingSimulationCommandHandler(IBackgroundJobClient backgroundJobClient, DataContext dataContext, IDateTimeProvider dateTimeProvider)
        {
            _backgroundJobClient = backgroundJobClient;
            _dataContext = dataContext;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<OperationResult<Unit>> Handle(StartingSimulationCommand request, CancellationToken cancellationToken)
        {
            var deviceIdFromRequest = request.DeviceGuid;

            var jobsFromDb = await _dataContext.SimulationJobs
               .Where(x => x.DeviceGuid == deviceIdFromRequest)
               .ToListAsync();

            if (!jobsFromDb.Any())
            {
                _result.AddError(ErrorCode.NotFound, string.Format(ErrorMessages.JobNotFound, deviceIdFromRequest));
                return _result;
            }

            #region RunningJobs stop if there is any
            var runningJobsFromHangfire = JobStorage.Current.GetMonitoringApi().ProcessingJobs(0, 1000);
            var jobsThatsRuningInHangFire = runningJobsFromHangfire.Select(x => Convert.ToInt32(x.Key));
            var hangFireJobsToStop = jobsFromDb
                .Select(x=>x.HangFireJobId)
                .Intersect(jobsThatsRuningInHangFire);

            var jobsToStop = jobsFromDb
                .Where(x => hangFireJobsToStop.Contains(x.HangFireJobId))
                .ToList();
                            
            foreach (var job in jobsToStop)
            {
                var state = new DeletedState();
                _backgroundJobClient.ChangeState(job.HangFireJobId.ToString(), state);

                job.UpdatedDate = _dateTimeProvider.DateTimeUtcNow;

                _dataContext.SimulationJobs.Update(job);
            }

            #endregion

            #region Schedule jobs 
            var sheduledJobsFromHangfire = JobStorage.Current.GetMonitoringApi().ScheduledJobs(0, 1000);
            var jobsThatsSheduledInHangFire = sheduledJobsFromHangfire.Select(x => Convert.ToInt32(x.Key));
            var hangFireJobsToShedule = jobsFromDb
                .Select(x => x.HangFireJobId)
                .Intersect(jobsThatsSheduledInHangFire);

            var jobsScheduled = jobsFromDb
                .Where(x => hangFireJobsToShedule.Contains(x.HangFireJobId))   
                .ToList();
                
            var jobsGroupedByRefence = jobsScheduled
                .GroupBy(x => x.ReferenceKey)                
                .ToDictionary(g => g.Key, g => g.ToList());

            #endregion

            foreach (var kpv in jobsGroupedByRefence)
            {
                var parentTime = _dateTimeProvider.DateTimeUtcNow;
                var sheduledTime = _dateTimeProvider.DateTimeUtcNow;

                foreach (var job in kpv.Value.Select((item, index) => (item, index)))
                {                    
                    if (job.index == 0)
                    {
                        var enqueuedState = new EnqueuedState();
                        _backgroundJobClient.ChangeState(job.item.HangFireJobId.ToString(), enqueuedState);

                        job.item.UpdatedDate = _dateTimeProvider.DateTimeUtcNow;

                        sheduledTime = parentTime.Add(job.item.Duration).ToUniversalTime();
                        parentTime = sheduledTime;
                        _dataContext.SimulationJobs.Update(job.item);
                        continue;
                    }
                    
                    var scheduledState = new ScheduledState(sheduledTime);                    
                    _backgroundJobClient.ChangeState(job.item.HangFireJobId.ToString(), scheduledState);
                    
                    job.item.UpdatedDate = _dateTimeProvider.DateTimeUtcNow;

                    sheduledTime = parentTime.Add(job.item.Duration).ToUniversalTime();
                    parentTime = sheduledTime;
                    _dataContext.SimulationJobs.Update(job.item);
                }
            }

            
            await _dataContext.SaveChangesAsync(cancellationToken);
            return _result;
        }
    }
}
