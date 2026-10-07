namespace SatinRoad.Domain;

/// <summary>Produces a number from 0 (inclusive) to 1 (exclusive). Swapped out in tests.</summary>
public interface IRaidRoller
{
    double Next();
}

/// <summary>The real roller: a random number.</summary>
public class RandomRaidRoller : IRaidRoller
{
    public double Next() => Random.Shared.NextDouble();
}

/// <summary>The FBI rule: each purchase has a small chance of being a raid.</summary>
public class RaidPolicy
{
    public double Chance { get; }

    public RaidPolicy(double chance)
    {
        if (chance is < 0 or > 1)
            throw new ArgumentOutOfRangeException(nameof(chance), chance, "The raid chance must be between 0 and 1.");
        Chance = chance;
    }

    /// <summary>With a chance of 0.01, a roll of 0.0099 is a raid and 0.01 is not.</summary>
    public bool IsRaid(IRaidRoller roller) => roller.Next() < Chance;
}