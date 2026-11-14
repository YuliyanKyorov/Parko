using Parko.Services;

namespace Parko.Tests;

public class ParkingPricingTests
{
    [Theory]
    [InlineData(-5, false, 0)]
    [InlineData(0, false, 0)]
    [InlineData(15, false, 0)]
    [InlineData(16, false, 1.2)]
    [InlineData(375, false, 7.2)]
    [InlineData(376, false, 8.2)]
    [InlineData(736, false, 25.2)]
    [InlineData(16, true, 1.2)]
    [InlineData(76, true, 2.4)]
    public void Calculate_applies_free_time_and_rate_periods(double minutes, bool isVip, decimal expected)
    {
        Assert.Equal(expected, ParkingPricing.Calculate(minutes, isVip));
    }

    [Fact]
    public void NormalizePlate_removes_separators_and_uses_invariant_uppercase()
    {
        Assert.Equal("AB123", ParkingService.NormalizePlate(" ab-123 "));
        Assert.Equal("АБ123", ParkingService.NormalizePlate("аб-123"));
        Assert.Equal(string.Empty, ParkingService.NormalizePlate(null));
    }

    [Theory]
    [InlineData("AB-123", true)]
    [InlineData("АБ 123", true)]
    [InlineData("ABC", false)]
    [InlineData("AB_123", false)]
    [InlineData("ABCDEFGHIJKLM", false)]
    public void IsValidPlate_checks_the_normalized_plate(string? plate, bool expected)
    {
        Assert.Equal(expected, ParkingService.IsValidPlate(plate));
    }
}
