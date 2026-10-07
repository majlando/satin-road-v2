using System.Net;
using System.Net.Http.Json;

namespace SatinRoad.Tests;

// API tests: each one boots the real API on a fresh database, sends real HTTP
// requests, and checks the answer and what ended up in the database.

public class AccessTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task No_acting_user_is_401()
    {
        await using var api = new ApiFactory();
        using var client = api.CreateClient();

        var response = await client.GetAsync("/api/users/me", Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task An_unknown_user_is_401()
    {
        await using var api = new ApiFactory();

        var response = await api.As(999).GetAsync("/api/users/me", Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task The_admin_exists_from_the_start()
    {
        await using var api = new ApiFactory();

        var users = await api.CreateClient().GetFromJsonAsync<List<UserDto>>("/api/users", Ct);

        users!.ShouldContain(u => u.Username == "admin" && u.Role == Roles.Admin);
    }

    [Fact]
    public async Task A_username_that_differs_only_in_case_is_taken()
    {
        await using var api = new ApiFactory();
        api.AddUser("ShadyPete");

        var response = await api.CreateClient().PostAsJsonAsync("/api/users", new CreateUserRequest("shadypete"), Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }
}

public class SellingTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task A_user_creates_a_listing()
    {
        await using var api = new ApiFactory();
        var vendor = api.AddUser("grandmasoap");
        var soap = api.AddCategory("Soap");

        var response = await api.As(vendor).PostAsJsonAsync("/api/my/listings",
            new ListingRequest(soap, "Lavender bar", "Smells nice", 999, 10), Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var listing = (await response.Content.ReadFromJsonAsync<ListingDto>(Ct))!;
        listing.VendorName.ShouldBe("grandmasoap");
        listing.CategoryName.ShouldBe("Soap");
    }

    [Theory]
    [InlineData("", 100, 1)]       // no title
    [InlineData("Bar", 0, 1)]      // free
    [InlineData("Bar", 100, -1)]   // negative stock
    public async Task An_invalid_listing_is_400(string title, long priceCents, int stock)
    {
        await using var api = new ApiFactory();
        var vendor = api.AddUser("grandmasoap");
        var soap = api.AddCategory("Soap");

        var response = await api.As(vendor).PostAsJsonAsync("/api/my/listings",
            new ListingRequest(soap, title, "", priceCents, stock), Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Someone_elses_listing_is_404_not_403()
    {
        await using var api = new ApiFactory();
        var owner = api.AddUser("grandmasoap");
        var other = api.AddUser("shadypete");
        var listing = api.AddListing(owner, api.AddCategory("Soap"), stock: 5);

        var response = await api.As(other).PatchAsJsonAsync($"/api/my/listings/{listing}/stock", new StockRequest(0), Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        api.StockOf(listing).ShouldBe(5);
    }

    [Fact]
    public async Task Deleting_is_a_soft_delete()
    {
        await using var api = new ApiFactory();
        var vendor = api.AddUser("grandmasoap");
        var listing = api.AddListing(vendor, api.AddCategory("Soap"));

        var response = await api.As(vendor).DeleteAsync($"/api/my/listings/{listing}", Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        api.Db(db => db.Listings.Single(l => l.Id == listing).IsRemoved).ShouldBeTrue();
    }
}

public class BrowseTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Removed_sold_out_and_seized_listings_are_hidden()
    {
        await using var api = new ApiFactory();
        var vendor = api.AddUser("grandmasoap");
        var seizedVendor = api.AddUser("shadypete");
        var category = api.AddCategory("Soap");

        var visible = api.AddListing(vendor, category, stock: 3);
        var removed = api.AddListing(vendor, category);
        var soldOut = api.AddListing(vendor, category, stock: 0);
        var fromSeized = api.AddListing(seizedVendor, category);
        api.Db(db => db.Listings.Where(l => l.Id == removed).Set(l => l.IsRemoved, true).Update());
        api.Db(db => db.Users.Where(u => u.Id == seizedVendor).Set(u => u.IsSeized, true).Update());

        var listings = await api.CreateClient().GetFromJsonAsync<List<ListingDto>>("/api/listings", Ct);

        listings!.Select(l => l.Id).ShouldBe([visible]);
    }
}