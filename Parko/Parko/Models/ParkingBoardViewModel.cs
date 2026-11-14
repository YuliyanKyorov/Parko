namespace Parko.Models;

public class ParkingBoardViewModel
{
    public ParkingSpaceMapViewModel SpaceMap { get; set; } = new();

    public List<Car> Cars { get; set; } = [];
}
