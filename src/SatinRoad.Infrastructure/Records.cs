namespace SatinRoad.Infrastructure;

/// <summary>
/// One class per table in schema.sql. These are storage rows, not API shapes:
/// controllers map them to DTOs so the wire format can change without a migration.
/// </summary>
[Table("users")]
public class UserRecord
{
    [PrimaryKey, Identity, Column("id")] public int Id { get; set; }
    [Column("username")] public string Username { get; set; } = "";

    /// <summary>One of <see cref="Roles"/>.</summary>
    [Column("role")] public string Role { get; set; } = Roles.User;

    /// <summary>Set by an FBI raid, and never unset: a raid is permanent.</summary>
    [Column("is_seized")] public bool IsSeized { get; set; }
}

[Table("categories")]
public class CategoryRecord
{
    [PrimaryKey, Identity, Column("id")] public int Id { get; set; }
    [Column("name")] public string Name { get; set; } = "";
}

[Table("listings")]
public class ListingRecord
{
    [PrimaryKey, Identity, Column("id")] public int Id { get; set; }
    [Column("vendor_id")] public int VendorId { get; set; }
    [Column("category_id")] public int CategoryId { get; set; }
    [Column("title")] public string Title { get; set; } = "";
    [Column("description")] public string Description { get; set; } = "";

    /// <summary>Whole cents. Always greater than zero.</summary>
    [Column("price_cents")] public long PriceCents { get; set; }

    [Column("stock")] public int Stock { get; set; }

    /// <summary>Soft delete, so orders keep pointing at something.</summary>
    [Column("is_removed")] public bool IsRemoved { get; set; }
}

[Table("orders")]
public class OrderRecord
{
    [PrimaryKey, Identity, Column("id")] public int Id { get; set; }
    [Column("buyer_id")] public int BuyerId { get; set; }

    /// <summary>Copied from the listing, so the order survives the listing being removed.</summary>
    [Column("vendor_id")] public int VendorId { get; set; }

    [Column("listing_id")] public int ListingId { get; set; }
    [Column("quantity")] public int Quantity { get; set; }

    [Column("subtotal_cents")] public long SubtotalCents { get; set; }
    [Column("discount_cents")] public long DiscountCents { get; set; }
    [Column("total_cents")] public long TotalCents { get; set; }

    /// <summary>One of <see cref="OrderStatus"/>.</summary>
    [Column("status")] public string Status { get; set; } = OrderStatus.Completed;
}