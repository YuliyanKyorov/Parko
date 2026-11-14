namespace Parko.Services;

public static class ParkingPricing
{
    public const int FreeMinutes = 15;
    public const decimal StandardFirstPeriodRate = 1.20m;
    public const decimal StandardSecondPeriodRate = 1.00m;
    public const decimal StandardDailyRate = 12.00m;
    public const decimal VipHourlyRate = 1.20m;
    public const int StandardFirstPeriodHours = 6;
    public const int StandardSecondPeriodHours = 6;
    public const int StandardDailyThresholdHours = 12;

    public static decimal Calculate(double totalMinutes, bool isVip)
    {
        var minutes = Math.Max(0d, totalMinutes);
        if (minutes <= FreeMinutes)
        {
            return 0m;
        }

        var billableMinutes = (decimal)(minutes - FreeMinutes);
        if (billableMinutes <= 0m)
        {
            return 0m;
        }

        if (isVip)
        {
            return CeilToHours(billableMinutes) * VipHourlyRate;
        }

        var firstPeriodMinutes = Math.Min(billableMinutes, StandardFirstPeriodHours * 60m);
        var secondPeriodMinutes = Math.Min(Math.Max(billableMinutes - StandardFirstPeriodHours * 60m, 0m), StandardSecondPeriodHours * 60m);
        var overflowMinutes = Math.Max(billableMinutes - StandardDailyThresholdHours * 60m, 0m);

        var total = CeilToHours(firstPeriodMinutes) * StandardFirstPeriodRate;
        total += CeilToHours(secondPeriodMinutes) * StandardSecondPeriodRate;
        total += CeilToDays(overflowMinutes) * StandardDailyRate;

        return total;
    }

    private static decimal CeilToHours(decimal totalMinutes)
        => decimal.Ceiling(totalMinutes / 60m);

    private static decimal CeilToDays(decimal totalMinutes)
        => decimal.Ceiling(totalMinutes / (24m * 60m));
}
