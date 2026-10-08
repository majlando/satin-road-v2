namespace SatinRoad.Api;

// The shapes the API sends and receives. Kept apart from the database records,
// so a column can change without changing what the browser sees.

public record UserDto(int Id, string Username, string Role, bool IsSeized)
{
    public static UserDto From(UserRecord u) => new(u.Id, u.Username, u.Role, u.IsSeized);
}

public record CreateUserRequest(string Username, string Password);

public record LoginRequest(string Username, string Password);

public record CategoryDto(int Id, string Name)
{
    public static CategoryDto From(CategoryRecord c) => new(c.Id, c.Name);
}

public record CategoryRequest(string Name);

/// <param name="VendorFeatured">The vendor has more than 100 sales; their listings are shown first.</param>
public record ListingDto(
    int Id, int VendorId, string VendorName, bool VendorFeatured, int CategoryId, string CategoryName,
    string Title, string Description, long PriceCents, int Stock);

public record ListingRequest(int CategoryId, string Title, string Description, long PriceCents, int Stock);

public record StockRequest(int Stock);

public record OrderRequest(int ListingId, int Quantity);

public record OrderDto(
    int Id, int ListingId, int Quantity,
    long SubtotalCents, long DiscountCents, long TotalCents, string Status)
{
    public static OrderDto From(OrderRecord o) =>
        new(o.Id, o.ListingId, o.Quantity, o.SubtotalCents, o.DiscountCents, o.TotalCents, o.Status);
}

public record FeaturedVendorDto(int VendorId, string VendorName, int Sales);

/// <summary>A vendor's shop. A seized vendor still has a page, but no listings.</summary>
public record VendorDto(int Id, string Name, bool IsSeized, int Sales, bool IsFeatured, List<ListingDto> Listings);
