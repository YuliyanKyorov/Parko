using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Parko.Models
{
    public class VipPoint
    {
        public const int Monthly = 40;
        public int Id { get; set; }
        public int ClientId { get; set; }
        [Range(0, Monthly)] public int Points { get; set; } = Monthly;
        public DateTime LastReset { get; set; } = DateTime.Now;
        [NotMapped]
        public int ChargingDays =>   // days until the 1st of the month
            (int)Math.Ceiling((new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(1) - DateTime.Now).TotalDays);


    }
}
