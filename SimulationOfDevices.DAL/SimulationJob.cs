using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SimulationOfDevices.DAL
{
    [Table("simulationjob")]
    public class SimulationJob
    {
        [Key]
        public int Id { get; set; }
        public int HangFireJobId { get; set; }

        [Column(TypeName = "guid")]
        public Guid DeviceGuid { get; set; }        
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public int QueuePosition { get; set; }
        public string ReferenceKey { get; set; }

        public TimeSpan Duration { get; set; }
    }
}
