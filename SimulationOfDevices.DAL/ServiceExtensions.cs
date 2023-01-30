using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SimulationOfDevices.DAL
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddPersistanceLayer(this IServiceCollection services, IConfiguration config)
        {

            var connectionString = config["ConnectionStrings:HangFireConnection"];
            services.AddDbContextPool<DataContext>
                (options => options.UseMySQL(connectionString));

            return services;
        }
    }
}
