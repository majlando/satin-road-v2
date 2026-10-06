namespace SatinRoad.Domain;

/// <summary>User roles. Stored as text in <c>users.role</c>.</summary>
public static class Roles
{
    public const string User = "User";
    public const string Admin = "Admin";
}

/// <summary>Order states. Stored as text in <c>orders.status</c>.</summary>
public static class OrderStatus
{
    public const string Completed = "Completed";

    /// <summary>The buyer turned out to be the FBI: no sale happened, and the vendor is shut down.</summary>
    public const string Seized = "Seized";
}