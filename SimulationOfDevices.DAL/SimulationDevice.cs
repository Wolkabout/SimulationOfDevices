using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SimulationOfDevices.DAL
{
    [Table("simulationdevice")]
    public class SimulationDevice
    {
        [Key]
        public int Id { get; set; }

        [Column(TypeName = "guid")]
        public Guid DeviceGuid { get; set; }

        [Column(TypeName = "json")]
        public string? Settings { get; set; }
    }
}