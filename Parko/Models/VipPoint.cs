using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Parko.Models
{
    public class VipPoint
    {
        public const int MonthlyPoints = 40;
        public int Id { get; set; }
        public int ClientId { get; set; }
        [Range(0, MonthlyPoints)] public int Points { get; set; } = MonthlyPoints;
        public DateTime LastReset { get; set; } = DateTime.Now;
        [NotMapped]
        public int ChargingDays =>   
            (int)Math.Ceiling((new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(1) - DateTime.Now).TotalDays);


    }
}
