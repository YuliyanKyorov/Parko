namespace Parko.Models;

public class ParkingSpaceMapViewModel
{
    public List<ParkingSpaceStatusViewModel> Spaces { get; set; } = [];

    public bool IsInteractive { get; set; }
}
