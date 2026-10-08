namespace SatinRoad.Api.Controllers;

[ApiController]
[Route("api/vendors")]
public class VendorsController(Catalog catalog) : ControllerBase
{
    /// <summary>Vendors with more than 100 completed sales, most sales first.</summary>
    [HttpGet("featured")]
    public Task<List<FeaturedVendorDto>> Featured() => catalog.FeaturedVendorsAsync();

    /// <summary>One vendor's shop, including sold-out listings. A seized vendor has none.</summary>
    [HttpGet("{id:int}")]
    public Task<VendorDto> Get(int id) => catalog.VendorAsync(id);
}
