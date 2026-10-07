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

/// <summary>The FBI raid chance.</summary>
public class RaidPolicyTests
{
    [Fact]
    public void A_roll_just_under_the_chance_is_a_raid()
    {
        new RaidPolicy(0.01).IsRaid(new FuncRoller(() => 0.0099)).ShouldBeTrue();
    }

    [Fact]
    public void A_roll_equal_to_the_chance_is_not_a_raid()
    {
        new RaidPolicy(0.01).IsRaid(new FuncRoller(() => 0.01)).ShouldBeFalse();
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(1.1)]
    public void A_chance_outside_zero_to_one_is_refused(double chance)
    {
        Should.Throw<ArgumentOutOfRangeException>(() => new RaidPolicy(chance));
    }

    [Fact]
    public void One_percent_is_about_one_percent()
    {
        // A fixed seed makes the "random" numbers the same on every run,
        // so this checks the distribution without ever failing by bad luck.
        var random = new Random(42);
        var roller = new FuncRoller(random.NextDouble);
        var policy = new RaidPolicy(0.01);

        var raids = Enumerable.Range(0, 100_000).Count(_ => policy.IsRaid(roller));

        raids.ShouldBeInRange(800, 1_200);
    }
}

/// <summary>A pretend dice roll that returns whatever the function returns, to force the outcome.</summary>
public class FuncRoller(Func<double> next) : IRaidRoller
{
    public double Next() => next();
}
/// <summary>The featured vendor threshold.</summary>
public class FeaturedVendorRuleTests
{
    [Fact]
    public void One_hundred_sales_is_not_featured() => FeaturedVendorRule.IsFeatured(100).ShouldBeFalse();

    [Fact]
    public void One_hundred_and_one_sales_is_featured() => FeaturedVendorRule.IsFeatured(101).ShouldBeTrue();
}