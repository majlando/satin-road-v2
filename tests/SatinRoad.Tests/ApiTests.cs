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

public class CategoryTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task A_normal_user_cannot_create_a_category()
    {
        await using var api = new ApiFactory();
        var user = api.AddUser("newbie");

        var response = await api.As(user).PostAsJsonAsync("/api/categories", new CategoryRequest("Soap"), Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task An_admin_creates_a_category_with_a_trimmed_name()
    {
        await using var api = new ApiFactory();
        var admin = api.AddUser("boss", Roles.Admin);

        var response = await api.As(admin).PostAsJsonAsync("/api/categories", new CategoryRequest("  Soap  "), Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        (await response.Content.ReadFromJsonAsync<CategoryDto>(Ct))!.Name.ShouldBe("Soap");
    }

    [Fact]
    public async Task An_empty_name_is_400()
    {
        await using var api = new ApiFactory();
        var admin = api.AddUser("boss", Roles.Admin);

        var response = await api.As(admin).PostAsJsonAsync("/api/categories", new CategoryRequest("   "), Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task A_duplicate_name_is_409()
    {
        await using var api = new ApiFactory();
        var admin = api.AddUser("boss", Roles.Admin);
        api.AddCategory("Soap");

        var response = await api.As(admin).PostAsJsonAsync("/api/categories", new CategoryRequest("soap"), Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task A_category_in_use_cannot_be_deleted()
    {
        await using var api = new ApiFactory();
        var admin = api.AddUser("boss", Roles.Admin);
        var vendor = api.AddUser("grandmasoap");
        var soap = api.AddCategory("Soap");
        api.AddListing(vendor, soap);

        var response = await api.As(admin).DeleteAsync($"/api/categories/{soap}", Ct);

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

public class BuyingTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task A_purchase_takes_the_stock_and_completes()
    {
        await using var api = new ApiFactory();
        var vendor = api.AddUser("grandmasoap");
        var buyer = api.AddUser("newbie");
        var listing = api.AddListing(vendor, api.AddCategory("Soap"), priceCents: 999, stock: 5);

        var response = await api.As(buyer).PostAsJsonAsync("/api/orders", new OrderRequest(listing, 2), Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var order = (await response.Content.ReadFromJsonAsync<OrderDto>(Ct))!;
        order.Status.ShouldBe(OrderStatus.Completed);
        order.TotalCents.ShouldBe(1_998);
        api.StockOf(listing).ShouldBe(3);
    }

    [Fact]
    public async Task The_twelfth_order_with_a_vendor_is_20_percent_off()
    {
        await using var api = new ApiFactory();
        var vendor = api.AddUser("grandmasoap");
        var buyer = api.AddUser("loyalbuyer");
        var listing = api.AddListing(vendor, api.AddCategory("Soap"), priceCents: 999, stock: 5);
        api.AddOrders(11, buyer, vendor, listing);

        var response = await api.As(buyer).PostAsJsonAsync("/api/orders", new OrderRequest(listing, 1), Ct);

        var order = (await response.Content.ReadFromJsonAsync<OrderDto>(Ct))!;
        order.DiscountCents.ShouldBe(200);
        order.TotalCents.ShouldBe(799);
    }

    [Fact]
    public async Task Seized_orders_do_not_count_towards_the_discount()
    {
        await using var api = new ApiFactory();
        var vendor = api.AddUser("grandmasoap");
        var buyer = api.AddUser("loyalbuyer");
        var listing = api.AddListing(vendor, api.AddCategory("Soap"), priceCents: 999, stock: 5);
        api.AddOrders(10, buyer, vendor, listing);
        api.AddOrders(5, buyer, vendor, listing, OrderStatus.Seized);

        var response = await api.As(buyer).PostAsJsonAsync("/api/orders", new OrderRequest(listing, 1), Ct);

        (await response.Content.ReadFromJsonAsync<OrderDto>(Ct))!.DiscountCents.ShouldBe(0);
    }

    [Fact]
    public async Task Buying_your_own_listing_is_400()
    {
        await using var api = new ApiFactory();
        var vendor = api.AddUser("grandmasoap");
        var listing = api.AddListing(vendor, api.AddCategory("Soap"), stock: 5);

        var response = await api.As(vendor).PostAsJsonAsync("/api/orders", new OrderRequest(listing, 1), Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        api.StockOf(listing).ShouldBe(5);
    }

    [Fact]
    public async Task A_quantity_below_one_is_400()
    {
        await using var api = new ApiFactory();
        var vendor = api.AddUser("grandmasoap");
        var buyer = api.AddUser("newbie");
        var listing = api.AddListing(vendor, api.AddCategory("Soap"), stock: 5);

        var response = await api.As(buyer).PostAsJsonAsync("/api/orders", new OrderRequest(listing, 0), Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        api.StockOf(listing).ShouldBe(5);
    }

    [Fact]
    public async Task More_than_the_stock_is_409()
    {
        await using var api = new ApiFactory();
        var vendor = api.AddUser("grandmasoap");
        var buyer = api.AddUser("newbie");
        var listing = api.AddListing(vendor, api.AddCategory("Soap"), stock: 2);

        var response = await api.As(buyer).PostAsJsonAsync("/api/orders", new OrderRequest(listing, 3), Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        api.StockOf(listing).ShouldBe(2);
    }
}

public class RaidTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task A_raid_seizes_the_order_and_shuts_the_vendor_down()
    {
        await using var api = new ApiFactory { NextRoll = 0 };   // 0 is always a raid
        var vendor = api.AddUser("shadypete");
        var buyer = api.AddUser("newbie");
        var category = api.AddCategory("Curiosities");
        var bought = api.AddListing(vendor, category, stock: 5);
        var other = api.AddListing(vendor, category, stock: 5);

        var response = await api.As(buyer).PostAsJsonAsync("/api/orders", new OrderRequest(bought, 1), Ct);

        (await response.Content.ReadFromJsonAsync<OrderDto>(Ct))!.Status.ShouldBe(OrderStatus.Seized);
        api.StockOf(bought).ShouldBe(5);                                             // no sale happened
        api.Db(db => db.Users.Single(u => u.Id == vendor).IsSeized).ShouldBeTrue();
        api.Db(db => db.Listings.Where(l => l.VendorId == vendor).All(l => l.IsRemoved)).ShouldBeTrue();
        api.Db(db => db.Listings.Single(l => l.Id == other).IsRemoved).ShouldBeTrue();
    }

    [Fact]
    public async Task After_a_raid_the_vendor_is_gone_for_good()
    {
        await using var api = new ApiFactory { NextRoll = 0 };
        var vendor = api.AddUser("shadypete");
        var buyer = api.AddUser("newbie");
        var listing = api.AddListing(vendor, api.AddCategory("Curiosities"), stock: 5);
        await api.As(buyer).PostAsJsonAsync("/api/orders", new OrderRequest(listing, 1), Ct);
        api.NextRoll = 0.99;   // no more raids from here on

        var browse = await api.CreateClient().GetFromJsonAsync<List<ListingDto>>("/api/listings", Ct);
        var buyAgain = await api.As(buyer).PostAsJsonAsync("/api/orders", new OrderRequest(listing, 1), Ct);
        var vendorEdits = await api.As(vendor).PostAsJsonAsync("/api/my/listings",
            new ListingRequest(1, "Comeback", "", 100, 1), Ct);

        browse.ShouldBeEmpty();
        buyAgain.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        vendorEdits.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }
}