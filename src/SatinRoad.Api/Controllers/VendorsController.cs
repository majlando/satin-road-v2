namespace SatinRoad.Api.Controllers;

[ApiController]
[Route("api/vendors")]
public class VendorsController(AppDb db) : ControllerBase
{
    /// <summary>Vendors with more than 100 completed sales, most sales first.</summary>
    [HttpGet("featured")]
    public async Task<List<FeaturedVendorDto>> Featured()
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
}