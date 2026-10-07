namespace SatinRoad.Api.Controllers;

/// <summary>Selling: the acting user's own listings.</summary>
[ApiController]
[Route("api/my/listings")]
public class MyListingsController(AppDb db, CurrentUser currentUser) : ControllerBase
{
    [HttpGet]
    public async Task<List<ListingDto>> List()
    {
        var me = await currentUser.GetAsync();
        return await Queries.VisibleListings(db)
            .Where(x => x.VendorId == me.Id)
            .OrderBy(x => x.Title)
            .ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<ListingDto>> Create(ListingRequest request)
    {
        var me = await currentUser.RequireActiveAsync();
        await ValidateAsync(request);

        var listing = new ListingRecord { VendorId = me.Id };
        Apply(listing, request);
        listing.Id = await db.InsertWithInt32IdentityAsync(listing);

        return Created($"/api/listings/{listing.Id}", await Queries.VisibleListings(db).FirstAsync(x => x.Id == listing.Id));
    }

    [HttpPut("{id:int}")]
    public async Task<ListingDto> Update(int id, ListingRequest request)
    {
        var listing = await FindMineAsync(id);
        await ValidateAsync(request);

        Apply(listing, request);
        await db.UpdateAsync(listing);
        return await Queries.VisibleListings(db).FirstAsync(x => x.Id == id);
    }

    [HttpPatch("{id:int}/stock")]
    public async Task<ListingDto> SetStock(int id, StockRequest request)
    {
        var listing = await FindMineAsync(id);
        if (request.Stock < 0)
            throw AppException.BadRequest("Stock cannot be negative.");

        listing.Stock = request.Stock;
        await db.UpdateAsync(listing);
        return await Queries.VisibleListings(db).FirstAsync(x => x.Id == id);
    }

    /// <summary>A soft delete: the row stays, so old orders still point at it.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var listing = await FindMineAsync(id);
        listing.IsRemoved = true;
        await db.UpdateAsync(listing);
        return NoContent();
    }

    /// <summary>
    /// One of the acting user's listings. Someone else's listing is a 404, not
    /// a 403, so nobody can find out which ids exist.
    /// </summary>
    private async Task<ListingRecord> FindMineAsync(int id)
    {
        var me = await currentUser.RequireActiveAsync();
        return await db.Listings.FirstOrDefaultAsync(l => l.Id == id && l.VendorId == me.Id && !l.IsRemoved)
            ?? throw AppException.NotFound("Listing");
    }

    private async Task ValidateAsync(ListingRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            throw AppException.BadRequest("Title is required.");
        if (request.PriceCents <= 0)
            throw AppException.BadRequest("Price must be more than zero.");
        if (request.Stock < 0)
            throw AppException.BadRequest("Stock cannot be negative.");
        if (!await db.Categories.AnyAsync(c => c.Id == request.CategoryId))
            throw AppException.BadRequest("That category does not exist.");
    }

    private static void Apply(ListingRecord listing, ListingRequest request)
    {
        listing.CategoryId = request.CategoryId;
        listing.Title = request.Title.Trim();
        listing.Description = request.Description.Trim();
        listing.PriceCents = request.PriceCents;
        listing.Stock = request.Stock;
    }
}