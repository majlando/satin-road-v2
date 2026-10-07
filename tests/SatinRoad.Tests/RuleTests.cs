namespace SatinRoad.Tests;

/// <summary>The loyalty discount. Pure function: no database, no HTTP.</summary>
public class PricingRulesTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(10)]   // exactly 10 is not "more than 10"
    public void No_discount_up_to_ten_earlier_orders(int earlierOrders)
    {
        var price = PricingRules.Calculate(10_000, earlierOrders);

        price.DiscountCents.ShouldBe(0);
        price.TotalCents.ShouldBe(10_000);
    }

    [Theory]
    [InlineData(11)]   // the first order that qualifies
    [InlineData(50)]
    public void Twenty_percent_off_after_more_than_ten(int earlierOrders)
    {
        var price = PricingRules.Calculate(10_000, earlierOrders);

        price.DiscountCents.ShouldBe(2_000);
        price.TotalCents.ShouldBe(8_000);
    }

    [Fact]
    public void Rounds_once_to_whole_cents()
    {
        // 20% of 999 is 199.8. Rounded to 200, the total is 799.
        var price = PricingRules.Calculate(999, 11);

        price.DiscountCents.ShouldBe(200);
        price.TotalCents.ShouldBe(799);
    }
}