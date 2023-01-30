using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimulationOfDevices.Services.BaseApiService;
using SimulationOfDevices.Services.Behaviors;
using SimulationOfDevices.Services.Common.Services;
using SimulationOfDevices.Services.RabbitMQ;
using SimulationOfDevices.Services.SettingsOptions;

namespace SimulationOfDevices.Services
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddServiceLayer(this IServiceCollection services, IConfiguration config)
        {
            SetUpCommonSettings(services, config);
            services.AddSingleton<IRabbitMqConfigService, RabbitMqConfigService>();

            services.AddMediatR(AppDomain.CurrentDomain.GetAssemblies());
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

            //services.Scan(s => s
            //    .FromAssemblyOf<IServiceMarker>()
            //    .AddClasses(f => f.Where(t => t.Name.EndsWith("Service")))
            //    .AsImplementedInterfaces()
            //    .WithTransientLifetime());
           
            services.AddTransient<IApiService, ApiService>();
            services.AddSingleton<IRabitMQProducer, RabitMQProducer>();
          
            return services;
        }

        private static IServiceCollection SetUpCommonSettings(IServiceCollection services, IConfiguration config)
        {
            services.Configure<RabbitMQSettings>(x => config.GetSection(nameof(RabbitMQSettings)).Bind(x));
            services.Configure<ApiServerSettings>(x => config.GetSection(nameof(ApiServerSettings)).Bind(x));

            return services;
        }
    }
}
