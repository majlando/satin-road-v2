namespace SatinRoad.Domain;

/// <summary>What an order costs, in whole cents.</summary>
public record Price(long SubtotalCents, long DiscountCents, long TotalCents);

/// <summary>
/// The loyalty rule: more than 10 completed orders with a vendor makes every
/// following order with that vendor 20% cheaper.
/// </summary>
public static class PricingRules
{
    public const int LoyaltyThreshold = 10;
    public const decimal LoyaltyDiscount = 0.20m;

    public static Price Calculate(long subtotalCents, int completedOrdersWithVendor)
    {
        if (completedOrdersWithVendor <= LoyaltyThreshold)
            return new Price(subtotalCents, 0, subtotalCents);

        // Round once, here, and away from zero: 999 cents → 199.8 → 200 off → 799.
        var discount = (long)Math.Round(subtotalCents * LoyaltyDiscount, MidpointRounding.AwayFromZero);
        return new Price(subtotalCents, discount, subtotalCents - discount);
    }
}