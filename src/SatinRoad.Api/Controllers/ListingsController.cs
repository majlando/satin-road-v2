namespace SatinRoad.Api.Controllers;

/// <summary>Browsing: what buyers see. No acting user needed.</summary>
[ApiController]
[Route("api/listings")]
public class ListingsController(AppDb db) : ControllerBase
{
    /// <summary>Listings that can be bought, optionally in one category.</summary>
    [HttpGet]
    public async Task<List<ListingDto>> Browse(int? categoryId)
    {
        var query = Queries.VisibleListings(db).Where(x => x.Stock > 0);
        if (categoryId is not null)
            query = query.Where(x => x.CategoryId == categoryId);

        return await query.OrderBy(x => x.Title).ToListAsync();
    }

    /// <summary>One listing, including a sold-out one.</summary>
    [HttpGet("{id:int}")]
    public async Task<ListingDto> Get(int id) =>
        await Queries.VisibleListings(db).FirstOrDefaultAsync(x => x.Id == id)
        ?? throw AppException.NotFound("Listing");
}

public static class Queries
{
    /// <summary>
    /// Listings that are not removed and whose vendor is not seized, joined with
    /// the vendor's and category's names.
    /// </summary>
    public static IQueryable<ListingDto> VisibleListings(AppDb db) =>
        from l in db.Listings
        join v in db.Users on l.VendorId equals v.Id
        join c in db.Categories on l.CategoryId equals c.Id
        where !l.IsRemoved && !v.IsSeized
        select new ListingDto(
            l.Id, l.VendorId, v.Username, l.CategoryId, c.Name,
            l.Title, l.Description, l.PriceCents, l.Stock);
}