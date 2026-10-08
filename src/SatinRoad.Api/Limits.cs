namespace SatinRoad.Api;

/// <summary>
/// Upper bounds on what a request may send. They keep rows a sane size, keep
/// password hashing cheap enough that it cannot be used to tie up the server,
/// and keep price × quantity far away from overflowing a long.
/// </summary>
public static class Limits
{
    public const int Username = 32;
    public const int Password = 128;
    public const int CategoryName = 50;
    public const int Title = 100;
    public const int Description = 2_000;

    /// <summary>$1,000,000.00, in cents.</summary>
    public const long PriceCents = 100_000_000;

    public const int Stock = 1_000_000;

    /// <summary>400 if <paramref name="value"/> is longer than <paramref name="max"/>.</summary>
    public static void MaxLength(string value, int max, string field)
    {
        if (value.Length > max)
            throw AppException.BadRequest($"{field} can be at most {max} characters.");
    }
}
