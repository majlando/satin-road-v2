namespace SatinRoad.Api.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController(AppDb db, CurrentUser currentUser) : ControllerBase
{
    /// <summary>Everyone can see the categories.</summary>
    [HttpGet]
    public async Task<List<CategoryDto>> List()
    {
        var categories = await db.Categories.OrderBy(c => c.Name).ToListAsync();
        return categories.Select(CategoryDto.From).ToList();
    }

    [HttpPost]
    public async Task<ActionResult<CategoryDto>> Create(CategoryRequest request)
    {
        await currentUser.RequireAdminAsync();
        var name = await ValidNameAsync(request.Name, exceptId: 0);

        var category = new CategoryRecord { Name = name };
        category.Id = await db.InsertWithInt32IdentityAsync(category);
        return Created($"/api/categories/{category.Id}", CategoryDto.From(category));
    }

    [HttpPut("{id:int}")]
    public async Task<CategoryDto> Rename(int id, CategoryRequest request)
    {
        await currentUser.RequireAdminAsync();
        var category = await db.Categories.FirstOrDefaultAsync(c => c.Id == id)
            ?? throw AppException.NotFound("Category");

        category.Name = await ValidNameAsync(request.Name, exceptId: id);
        await db.UpdateAsync(category);
        return CategoryDto.From(category);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await currentUser.RequireAdminAsync();
        if (!await db.Categories.AnyAsync(c => c.Id == id))
            throw AppException.NotFound("Category");

        // Removed listings count too: their old orders still point at them.
        if (await db.Listings.AnyAsync(l => l.CategoryId == id))
            throw AppException.Conflict("This category still has listings.");

        await db.Categories.Where(c => c.Id == id).DeleteAsync();
        return NoContent();
    }

    /// <summary>Trims the name, and refuses an empty one (400) or a taken one (409).</summary>
    private async Task<string> ValidNameAsync(string raw, int exceptId)
    {
        var name = raw.Trim();
        if (name.Length == 0)
            throw AppException.BadRequest("Name is required.");
        Limits.MaxLength(name, Limits.CategoryName, "Name");

        // The column is COLLATE NOCASE, so "soap" and "Soap" count as the same.
        if (await db.Categories.AnyAsync(c => c.Name == name && c.Id != exceptId))
            throw AppException.Conflict($"A category called '{name}' already exists.");

        return name;
    }
}