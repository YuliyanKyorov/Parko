using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.ConstrainedExecution;

namespace Parko.Models
{
    public class ParkingSpace
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None), Range(1, 100)] public int Id { get; set; }
        public List<Car> Cars { get; set; } = new();
        [NotMapped] public Car? Current => Cars.FirstOrDefault(static c => c.ReleaseTime == null);



    }
}
