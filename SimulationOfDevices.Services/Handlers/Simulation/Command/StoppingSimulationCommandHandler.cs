using Hangfire;
using Hangfire.States;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SimulationOfDevices.DAL;
using SimulationOfDevices.Services.Common.Services;
using SimulationOfDevices.Services.Enums;
using SimulationOfDevices.Services.Models;

namespace SimulationOfDevices.Services.Handlers.Simulation.Command
{
    public record StoppingSimulationCommand : IRequest<OperationResult<Unit>>
    {
        public Guid DeviceGuid { get; set; }

        public StoppingSimulationCommand(Guid deviceGuid)
        {
            DeviceGuid = deviceGuid;
        }
    }

    public sealed class StoppingSimulationCommandHandler : IRequestHandler<StoppingSimulationCommand, OperationResult<Unit>>
    {
        private readonly OperationResult<Unit> _result = new();
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly DataContext _dataContext;
        private readonly IDateTimeProvider _dateTimeProvider;

        public StoppingSimulationCommandHandler(IBackgroundJobClient backgroundJobClient, DataContext dataContext, IDateTimeProvider dateTimeProvider)
        {
            _backgroundJobClient = backgroundJobClient;
            _dataContext = dataContext;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<OperationResult<Unit>> Handle(StoppingSimulationCommand request, CancellationToken cancellationToken)
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

            var runningJobsFromHangfire = JobStorage.Current.GetMonitoringApi().ProcessingJobs(0, 1000);
            var jobsThatsRuningInHangFire = runningJobsFromHangfire.Select(x => Convert.ToInt32(x.Key));
            var hangFireJobsToStop = jobsFromDb
                .Select(x => x.HangFireJobId)
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

            await _dataContext.SaveChangesAsync(cancellationToken);
            return _result;
        }
    }
}
