using Hangfire;
using Hangfire.States;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SimulationOfDevices.DAL;
using SimulationOfDevices.Services.Common;
using SimulationOfDevices.Services.Models;
using SimulationOfDevices.Services.Models.JobModels;
using SimulationOfDevices.Services.RabbitMQ;

namespace SimulationOfDevices.Services.Handlers.Simulation.Command
{
    public class SettingUpSimulationCommand : IRequest<OperationResult<Unit>>
    {
        public SimulationForDeviceId Model { get; set; }
        public SettingUpSimulationCommand(SimulationForDeviceId model)
        {
            Model = model;
        }
    }

    public class SettingUpSimulationCommandHandler : IRequestHandler<SettingUpSimulationCommand, OperationResult<Unit>>
    {        
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly OperationResult<Unit> _result = new();

        private readonly DataContext _dataContext;

        public SettingUpSimulationCommandHandler(IBackgroundJobClient backgroundJobClient, DataContext dataContext)
        {            
            _backgroundJobClient = backgroundJobClient;
            _dataContext = dataContext;
        }


        public async Task<OperationResult<Unit>> Handle(SettingUpSimulationCommand request, CancellationToken cancellationToken)
        {                    
            var simulationDevice = await _dataContext.SimulationDevices.FirstOrDefaultAsync(x=> x.DeviceGuid == request.Model.DeviceGuid);
            if (simulationDevice != null)
            {                
                simulationDevice.Settings = null;
                
                await _dataContext.SaveChangesAsync();
                return _result;
            }
                                    
            _dataContext.SimulationDevices.Add(new SimulationDevice()
            {                
                DeviceGuid = request.Model.DeviceGuid,
                Settings = null                
            });

            await _dataContext.SaveChangesAsync();            
            return _result;
        }
    }
}
