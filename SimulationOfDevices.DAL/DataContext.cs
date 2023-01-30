using Microsoft.EntityFrameworkCore;

namespace SimulationOfDevices.DAL
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions options) : base(options)
        {            
        }

        public DbSet<SimulationDevice> SimulationDevices { get; set; }
        public DbSet<SimulationJob> SimulationJobs { get; set; }
    }
}
