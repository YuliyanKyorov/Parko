namespace Parko.Models
{
    public class Car
    {
        public int Id { get; set; }
        public string Vehicle { get; set; } = "";
        public string Model { get; set; } = "";
        public DateTime EntryTime { get; set; }
        public DateTime? ReleaseTime { get; set; }
        public decimal? Price { get; set; }
        public int ParkingSpaceId { get; set; }
        public ParkingSpace? ParkingSpace { get; set; }
        public bool IsVip { get; set; }
        public bool UsedPoint { get; set; }

    }
}