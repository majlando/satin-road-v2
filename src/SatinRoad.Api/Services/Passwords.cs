using Microsoft.AspNetCore.Identity;

namespace SatinRoad.Api.Services;

/// <summary>
/// Hashing and checking passwords, with ASP.NET Core's PasswordHasher (PBKDF2,
/// salted). Only the hash is ever stored.
/// </summary>
public static class Passwords
{
    /// <summary>The password every seeded user gets, so the demo can be logged into.</summary>
    public const string Demo = "password";

    public const int MinLength = 8;

    private static readonly PasswordHasher<UserRecord> Hasher = new();

    public static string Hash(string password) => Hasher.HashPassword(null!, password);

    /// <summary>True if the password matches. A user without a hash never matches.</summary>
    public static bool Verify(UserRecord user, string password) =>
        user.PasswordHash.Length > 0
        && Hasher.VerifyHashedPassword(user, user.PasswordHash, password) != PasswordVerificationResult.Failed;
}
