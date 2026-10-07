namespace SatinRoad.Domain;

/// <summary>The featured rule: a vendor with more than 100 completed sales is featured.</summary>
public static class FeaturedVendorRule
{
    public const int Threshold = 100;

    public static bool IsFeatured(int completedSales) => completedSales > Threshold;
}