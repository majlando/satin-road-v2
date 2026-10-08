using System.Net;
using System.Net.Http.Json;

namespace SatinRoad.Tests;

// API tests: each one boots the real API on a fresh database, sends real HTTP
// requests, and checks the answer and what ended up in the database.

public class AccessTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Nobody_logged_in_is_401()
    {
        await using var api = new ApiFactory();
        using var client = api.CreateClient();

        var response = await client.GetAsync("/api/users/me", Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task A_forged_user_id_header_is_ignored()
    {
        await using var api = new ApiFactory();
        var user = api.AddUser("vitocorleone");
        using var client = api.CreateClient();
        client.DefaultRequestHeaders.Add("X-User-Id", user.ToString());

        var response = await client.GetAsync("/api/users/me", Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task The_admin_exists_from_the_start()
    {
        await using var api = new ApiFactory();

        var login = await api.CreateClient().PostAsJsonAsync("/api/auth/login", new LoginRequest("admin", Passwords.Demo), Ct);
        var admin = await login.Content.ReadFromJsonAsync<UserDto>(Ct);

        admin!.Role.ShouldBe(Roles.Admin);
    }

    [Fact]
    public async Task The_list_of_users_is_not_public()
    {
        await using var api = new ApiFactory();

        var response = await api.CreateClient().GetAsync("/api/users", Ct);

        response.IsSuccessStatusCode.ShouldBeFalse();
    }

    [Fact]
    public async Task A_username_that_differs_only_in_case_is_taken()
    {
        await using var api = new ApiFactory();
        api.AddUser("VitoCorleone");

        var response = await api.CreateClient().PostAsJsonAsync("/api/users", new CreateUserRequest("vitocorleone", "longenough"), Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }
}

public class AuthTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task The_right_password_logs_in()
    {
        await using var api = new ApiFactory();
        api.AddUser("vitocorleone");
        using var client = api.CreateClient();

        var login = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest("VitoCorleone", Passwords.Demo), Ct);
        var me = await client.GetFromJsonAsync<UserDto>("/api/users/me", Ct);

        login.StatusCode.ShouldBe(HttpStatusCode.OK);
        me!.Username.ShouldBe("vitocorleone");
    }

    [Fact]
    public async Task A_wrong_password_is_401()
    {
        await using var api = new ApiFactory();
        api.AddUser("vitocorleone");

        var response = await api.CreateClient().PostAsJsonAsync("/api/auth/login", new LoginRequest("vitocorleone", "not-it-at-all"), Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task An_unknown_username_is_401()
    {
        await using var api = new ApiFactory();

        var response = await api.CreateClient().PostAsJsonAsync("/api/auth/login", new LoginRequest("nobody", Passwords.Demo), Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task A_user_without_a_password_cannot_log_in()
    {
        await using var api = new ApiFactory();
        api.Db(db => db.InsertWithInt32Identity(new UserRecord { Username = "nopassword" }));

        var response = await api.CreateClient().PostAsJsonAsync("/api/auth/login", new LoginRequest("nopassword", ""), Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logging_out_forgets_the_user()
    {
        await using var api = new ApiFactory();
        using var client = api.As(api.AddUser("vitocorleone"));

        var logout = await client.PostAsync("/api/auth/logout", null, Ct);
        var me = await client.GetAsync("/api/users/me", Ct);

        logout.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        me.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Registering_logs_the_new_user_in()
    {
        await using var api = new ApiFactory();
        using var client = api.CreateClient();

        var created = await client.PostAsJsonAsync("/api/users", new CreateUserRequest("jacksparrow", "longenough"), Ct);
        var me = await client.GetFromJsonAsync<UserDto>("/api/users/me", Ct);

        created.StatusCode.ShouldBe(HttpStatusCode.Created);
        me!.Username.ShouldBe("jacksparrow");
        me.Role.ShouldBe(Roles.User);
    }

    [Fact]
    public async Task A_short_password_is_400()
    {
        await using var api = new ApiFactory();

        var response = await api.CreateClient().PostAsJsonAsync("/api/users", new CreateUserRequest("jacksparrow", "short"), Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task An_over_long_username_or_password_is_400()
    {
        await using var api = new ApiFactory();
        var client = api.CreateClient();

        var longName = await client.PostAsJsonAsync("/api/users", new CreateUserRequest(new string('a', Limits.Username + 1), "longenough"), Ct);
        var longPassword = await client.PostAsJsonAsync("/api/users", new CreateUserRequest("jacksparrow", new string('a', Limits.Password + 1)), Ct);

        longName.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        longPassword.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
}

public class CategoryTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task A_normal_user_cannot_create_a_category()
    {
        await using var api = new ApiFactory();
        var user = api.AddUser("jacksparrow");

        var response = await api.As(user).PostAsJsonAsync("/api/categories", new CategoryRequest("Hard Drugs"), Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task An_admin_creates_a_category_with_a_trimmed_name()
    {
        await using var api = new ApiFactory();
        var admin = api.AddUser("boss", Roles.Admin);

        var response = await api.As(admin).PostAsJsonAsync("/api/categories", new CategoryRequest("  Hard Drugs  "), Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        (await response.Content.ReadFromJsonAsync<CategoryDto>(Ct))!.Name.ShouldBe("Hard Drugs");
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
        api.AddCategory("Hard Drugs");

        var response = await api.As(admin).PostAsJsonAsync("/api/categories", new CategoryRequest("hard drugs"), Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task A_category_in_use_cannot_be_deleted()
    {
        await using var api = new ApiFactory();
        var admin = api.AddUser("boss", Roles.Admin);
        var vendor = api.AddUser("walterwhite");
        var drugs = api.AddCategory("Hard Drugs");
        api.AddListing(vendor, drugs);

        var response = await api.As(admin).DeleteAsync($"/api/categories/{drugs}", Ct);

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
        var vendor = api.AddUser("walterwhite");
        var drugs = api.AddCategory("Hard Drugs");

        var response = await api.As(vendor).PostAsJsonAsync("/api/my/listings",
            new ListingRequest(drugs, "Blue rock candy", "99.1% pure", 999, 10), Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var listing = (await response.Content.ReadFromJsonAsync<ListingDto>(Ct))!;
        listing.VendorName.ShouldBe("walterwhite");
        listing.CategoryName.ShouldBe("Hard Drugs");
    }

    [Theory]
    [InlineData("", 100, 1)]       // no title
    [InlineData("Candy", 0, 1)]      // free
    [InlineData("Candy", 100, -1)]   // negative stock
    [InlineData("Candy", Limits.PriceCents + 1, 1)]  // too expensive
    [InlineData("Candy", 100, Limits.Stock + 1)]     // too much stock
    public async Task An_invalid_listing_is_400(string title, long priceCents, int stock)
    {
        await using var api = new ApiFactory();
        var vendor = api.AddUser("walterwhite");
        var drugs = api.AddCategory("Hard Drugs");

        var response = await api.As(vendor).PostAsJsonAsync("/api/my/listings",
            new ListingRequest(drugs, title, "", priceCents, stock), Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Someone_elses_listing_is_404_not_403()
    {
        await using var api = new ApiFactory();
        var owner = api.AddUser("walterwhite");
        var other = api.AddUser("vitocorleone");
        var listing = api.AddListing(owner, api.AddCategory("Hard Drugs"), stock: 5);

        var response = await api.As(other).PatchAsJsonAsync($"/api/my/listings/{listing}/stock", new StockRequest(0), Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        api.StockOf(listing).ShouldBe(5);
    }

    [Fact]
    public async Task Deleting_is_a_soft_delete()
    {
        await using var api = new ApiFactory();
        var vendor = api.AddUser("walterwhite");
        var listing = api.AddListing(vendor, api.AddCategory("Hard Drugs"));

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
        var vendor = api.AddUser("walterwhite");
        var seizedVendor = api.AddUser("vitocorleone");
        var category = api.AddCategory("Hard Drugs");

        var visible = api.AddListing(vendor, category, stock: 3);
        var removed = api.AddListing(vendor, category);
        var soldOut = api.AddListing(vendor, category, stock: 0);
        var fromSeized = api.AddListing(seizedVendor, category);
        api.Db(db => db.Listings.Where(l => l.Id == removed).Set(l => l.IsRemoved, true).Update());
        api.Db(db => db.Users.Where(u => u.Id == seizedVendor).Set(u => u.IsSeized, true).Update());

        var listings = await api.CreateClient().GetFromJsonAsync<List<ListingDto>>("/api/listings", Ct);

        listings!.Select(l => l.Id).ShouldBe([visible]);
    }

    [Fact]
    public async Task Featured_vendors_listings_come_first()
    {
        await using var api = new ApiFactory();
        var buyer = api.AddUser("bonnieparker");
        var category = api.AddCategory("Military Hardware");
        var star = api.AddUser("vitocorleone");
        var normal = api.AddUser("alcapone");
        var starListing = api.AddListing(star, category, title: "Zebra");
        var normalListing = api.AddListing(normal, category, title: "Apple");
        api.AddOrders(101, buyer, star, starListing);

        var listings = await api.CreateClient().GetFromJsonAsync<List<ListingDto>>("/api/listings", Ct);

        listings!.Select(l => l.Id).ShouldBe([starListing, normalListing]);
        listings!.Select(l => l.VendorFeatured).ShouldBe([true, false]);
    }

    [Fact]
    public async Task A_vendor_page_shows_sold_out_listings_and_sales()
    {
        await using var api = new ApiFactory();
        var buyer = api.AddUser("tonymontana");
        var vendor = api.AddUser("walterwhite");
        var category = api.AddCategory("Hard Drugs");
        var inStock = api.AddListing(vendor, category, stock: 3);
        var soldOut = api.AddListing(vendor, category, stock: 0);
        api.AddOrders(4, buyer, vendor, inStock);

        var shop = await api.CreateClient().GetFromJsonAsync<VendorDto>($"/api/vendors/{vendor}", Ct);

        shop!.Name.ShouldBe("walterwhite");
        shop.Sales.ShouldBe(4);
        shop.IsFeatured.ShouldBeFalse();
        shop.Listings.Select(l => l.Id).ShouldBe([inStock, soldOut], ignoreOrder: true);
    }

    [Fact]
    public async Task A_seized_vendor_page_is_empty_and_an_unknown_one_is_404()
    {
        await using var api = new ApiFactory();
        var vendor = api.AddUser("vitocorleone");
        api.AddListing(vendor, api.AddCategory("Military Hardware"));
        api.Db(db => db.Users.Where(u => u.Id == vendor).Set(u => u.IsSeized, true).Update());
        var client = api.CreateClient();

        var shop = await client.GetFromJsonAsync<VendorDto>($"/api/vendors/{vendor}", Ct);
        var unknown = await client.GetAsync("/api/vendors/9999", Ct);

        shop!.IsSeized.ShouldBeTrue();
        shop.Listings.ShouldBeEmpty();
        unknown.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}

public class BuyingTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task A_purchase_takes_the_stock_and_completes()
    {
        await using var api = new ApiFactory();
        var vendor = api.AddUser("walterwhite");
        var buyer = api.AddUser("jacksparrow");
        var listing = api.AddListing(vendor, api.AddCategory("Hard Drugs"), priceCents: 999, stock: 5);

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
        var vendor = api.AddUser("walterwhite");
        var buyer = api.AddUser("tonymontana");
        var listing = api.AddListing(vendor, api.AddCategory("Hard Drugs"), priceCents: 999, stock: 5);
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
        var vendor = api.AddUser("walterwhite");
        var buyer = api.AddUser("tonymontana");
        var listing = api.AddListing(vendor, api.AddCategory("Hard Drugs"), priceCents: 999, stock: 5);
        api.AddOrders(10, buyer, vendor, listing);
        api.AddOrders(5, buyer, vendor, listing, OrderStatus.Seized);

        var response = await api.As(buyer).PostAsJsonAsync("/api/orders", new OrderRequest(listing, 1), Ct);

        (await response.Content.ReadFromJsonAsync<OrderDto>(Ct))!.DiscountCents.ShouldBe(0);
    }

    [Fact]
    public async Task Buying_your_own_listing_is_400()
    {
        await using var api = new ApiFactory();
        var vendor = api.AddUser("walterwhite");
        var listing = api.AddListing(vendor, api.AddCategory("Hard Drugs"), stock: 5);

        var response = await api.As(vendor).PostAsJsonAsync("/api/orders", new OrderRequest(listing, 1), Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        api.StockOf(listing).ShouldBe(5);
    }

    [Fact]
    public async Task A_quantity_below_one_is_400()
    {
        await using var api = new ApiFactory();
        var vendor = api.AddUser("walterwhite");
        var buyer = api.AddUser("jacksparrow");
        var listing = api.AddListing(vendor, api.AddCategory("Hard Drugs"), stock: 5);

        var response = await api.As(buyer).PostAsJsonAsync("/api/orders", new OrderRequest(listing, 0), Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        api.StockOf(listing).ShouldBe(5);
    }

    [Fact]
    public async Task More_than_the_stock_is_409()
    {
        await using var api = new ApiFactory();
        var vendor = api.AddUser("walterwhite");
        var buyer = api.AddUser("jacksparrow");
        var listing = api.AddListing(vendor, api.AddCategory("Hard Drugs"), stock: 2);

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
        var vendor = api.AddUser("vitocorleone");
        var buyer = api.AddUser("jacksparrow");
        var category = api.AddCategory("Military Hardware");
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
        var vendor = api.AddUser("vitocorleone");
        var buyer = api.AddUser("jacksparrow");
        var listing = api.AddListing(vendor, api.AddCategory("Military Hardware"), stock: 5);
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

public class FeaturedTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Only_vendors_with_more_than_100_completed_sales_are_featured()
    {
        await using var api = new ApiFactory();
        var buyer = api.AddUser("bonnieparker");
        var category = api.AddCategory("Military Hardware");
        var star = api.AddUser("vitocorleone");
        var almost = api.AddUser("walterwhite");
        api.AddOrders(101, buyer, star, api.AddListing(star, category));
        api.AddOrders(100, buyer, almost, api.AddListing(almost, category));

        var featured = await api.CreateClient().GetFromJsonAsync<List<FeaturedVendorDto>>("/api/vendors/featured", Ct);

        featured!.Select(f => f.VendorName).ShouldBe(["vitocorleone"]);
        featured![0].Sales.ShouldBe(101);
    }

    [Fact]
    public async Task Seized_orders_do_not_count_and_seized_vendors_are_never_featured()
    {
        await using var api = new ApiFactory();
        var buyer = api.AddUser("bonnieparker");
        var category = api.AddCategory("Military Hardware");
        var raided = api.AddUser("vitocorleone");
        var padded = api.AddUser("walterwhite");
        api.AddOrders(150, buyer, raided, api.AddListing(raided, category));
        api.Db(db => db.Users.Where(u => u.Id == raided).Set(u => u.IsSeized, true).Update());
        var paddedListing = api.AddListing(padded, category);
        api.AddOrders(100, buyer, padded, paddedListing);
        api.AddOrders(50, buyer, padded, paddedListing, OrderStatus.Seized);

        var featured = await api.CreateClient().GetFromJsonAsync<List<FeaturedVendorDto>>("/api/vendors/featured", Ct);

        featured.ShouldBeEmpty();
    }
}