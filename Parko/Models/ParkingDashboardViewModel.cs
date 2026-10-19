namespace Parko.Models;

public class ParkingDashboardViewModel
{
    public List<int> ParkingSpaces { get; set; } = [];

    public List<int> AvailableSpaces { get; set; } = [];

    public List<int> OccupiedSpaces { get; set; } = [];

    public List<int> VipOccupiedSpaces { get; set; } = [];

    public ParkCarViewModel CustomerParking { get; set; } = new();

    public CheckInVm AdminCheckIn { get; set; } = new();

    public bool IsAdmin { get; set; }
}
