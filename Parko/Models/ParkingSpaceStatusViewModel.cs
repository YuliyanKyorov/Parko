namespace Parko.Models;

public class ParkingSpaceStatusViewModel
{
    public int Id { get; set; }

    public bool IsOccupied { get; set; }

    public bool IsVip { get; set; }

    public string? LicensePlate { get; set; }

    public string? VehicleModel { get; set; }
}
