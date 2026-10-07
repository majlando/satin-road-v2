namespace SatinRoad.Domain;

public record Price(long SubtotalCents, long DiscountCents, long TotalCents);

public static class PricingRules
{
    // Stub: never discounts. Replaced in the next commit.
    public static Price Calculate(long subtotalCents, int completedOrdersWithVendor) =>
        new(subtotalCents, 0, subtotalCents);
}