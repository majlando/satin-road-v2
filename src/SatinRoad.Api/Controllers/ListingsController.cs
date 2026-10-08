namespace SatinRoad.Api.Controllers;

/// <summary>Browsing: what buyers see. No acting user needed.</summary>
[ApiController]
[Route("api/listings")]
public class ListingsController(Catalog catalog) : ControllerBase
{
    /// <summary>Listings that can be bought, optionally in one category. Featured vendors first.</summary>
    [HttpGet]
    public Task<List<ListingDto>> Browse(int? categoryId) =>
        catalog.ListingsAsync(q =>
        {
            q = q.Where(x => x.Stock > 0);
            return categoryId is null ? q : q.Where(x => x.CategoryId == categoryId);
        });

    /// <summary>One listing, including a sold-out one.</summary>
    [HttpGet("{id:int}")]
    public Task<ListingDto> Get(int id) => catalog.ListingAsync(id);
}
