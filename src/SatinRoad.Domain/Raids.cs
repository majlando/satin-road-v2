namespace SatinRoad.Domain;

public interface IRaidRoller
{
    double Next();
}

public class RaidPolicy(double chance)
{
    public double Chance { get; } = chance;

    // Stub: never raids. Replaced in the next commit.
    public bool IsRaid(IRaidRoller roller) => false;
}