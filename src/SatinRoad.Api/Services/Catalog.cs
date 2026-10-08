namespace SatinRoad.Api.Services;

/// <summary>
/// What buyers can see: visible listings and featured vendors. Every read
/// endpoint goes through here, so "visible" and "featured" mean the same thing
/// everywhere, and the featured-first order is decided by the API, not the browser.
/// </summary>
public class Catalog(AppDb db)
{
    /// <summary>
    /// Visible listings matching <paramref name="filter"/>, each marked with
    /// whether its vendor is featured. Featured vendors' listings come first,
    /// then everything by title.
    /// </summary>
    public async Task<List<ListingDto>> ListingsAsync(Func<IQueryable<ListingDto>, IQueryable<ListingDto>> filter)
    {
        var listings = await filter(Visible()).ToListAsync();
        var featured = (await FeaturedVendorsAsync()).Select(f => f.VendorId).ToHashSet();

        return listings
            .Select(l => l with { VendorFeatured = featured.Contains(l.VendorId) })
            .OrderByDescending(l => l.VendorFeatured)
            .ThenBy(l => l.Title)
            .ToList();
    }

    /// <summary>One visible listing, including a sold-out one. 404 otherwise.</summary>
    public async Task<ListingDto> ListingAsync(int id) =>
        (await ListingsAsync(q => q.Where(x => x.Id == id))).FirstOrDefault()
        ?? throw AppException.NotFound("Listing");

    /// <summary>Vendors with more than 100 completed sales, most sales first.</summary>
    public async Task<List<FeaturedVendorDto>> FeaturedVendorsAsync()
    {
        // The database counts completed sales per vendor (GROUP BY)...
        var sales = await (
            from o in db.Orders
            join v in db.Users on o.VendorId equals v.Id
            where o.Status == OrderStatus.Completed && !v.IsSeized
            group o by new { v.Id, v.Username } into g
            select new { g.Key.Id, g.Key.Username, Sales = g.Count() }
        ).ToListAsync();

        // ...and the rule decides who is featured, so "more than 100" lives in one place.
        return sales
            .Where(s => FeaturedVendorRule.IsFeatured(s.Sales))
            .OrderByDescending(s => s.Sales)
            .Select(s => new FeaturedVendorDto(s.Id, s.Username, s.Sales))
            .ToList();
    }

    /// <summary>A vendor's shop: who they are, their sales, and what they sell. 404 for an unknown id.</summary>
    public async Task<VendorDto> VendorAsync(int id)
    {
        var vendor = await db.Users.FirstOrDefaultAsync(u => u.Id == id)
                     ?? throw AppException.NotFound("Vendor");

        var sales = await db.Orders.CountAsync(o => o.VendorId == id && o.Status == OrderStatus.Completed);
        var listings = await ListingsAsync(q => q.Where(x => x.VendorId == id));

        return new VendorDto(
            vendor.Id, vendor.Username, vendor.IsSeized, sales,
            IsFeatured: !vendor.IsSeized && FeaturedVendorRule.IsFeatured(sales),
            listings);
    }

    /// <summary>
    /// Listings that are not removed and whose vendor is not seized, joined with
    /// the vendor's and category's names. Featured is filled in afterwards.
    /// </summary>
    private IQueryable<ListingDto> Visible() =>
        from l in db.Listings
        join v in db.Users on l.VendorId equals v.Id
        join c in db.Categories on l.CategoryId equals c.Id
        where !l.IsRemoved && !v.IsSeized
        select new ListingDto(
            l.Id, l.VendorId, v.Username, false, l.CategoryId, c.Name,
            l.Title, l.Description, l.PriceCents, l.Stock);
}
