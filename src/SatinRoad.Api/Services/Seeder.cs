using LinqToDB.Data;

namespace SatinRoad.Api.Services;

/// <summary>
/// Fills the database at startup. The admin is always created. The demo data,
/// which sets up every rule so it can be shown straight away, is only added
/// when Seed:Demo is true and there are no listings yet.
/// </summary>
public static class Seeder
{
    public static void Run(AppDb db, bool demo)
    {
        if (!db.Users.Any(u => u.Username == "admin"))
            db.Insert(new UserRecord { Username = "admin", Role = Roles.Admin });

        if (!demo || db.Listings.Any()) return;

        // Users. Vendors sell; buyers buy.
        var shadypete   = AddUser(db, "shadypete");     // featured: 101 sales
        var grandmasoap = AddUser(db, "grandmasoap");   // loyalbuyer's favourite
        var oddjobs     = AddUser(db, "oddjobs");       // a normal vendor
        var loyalbuyer  = AddUser(db, "loyalbuyer");    // 11 orders at grandmasoap
        var bulkbuyer   = AddUser(db, "bulkbuyer");     // bought shadypete's 101
        AddUser(db, "newbie");                          // fresh, for the raid demo

        // Categories.
        var curiosities = AddCategory(db, "Curiosities");
        var soap        = AddCategory(db, "Handmade Soap");
        var artifacts   = AddCategory(db, "Stolen Artifacts");
        var snacks      = AddCategory(db, "Suspicious Snacks");
        var electronics = AddCategory(db, "Vintage Electronics");

        // Listings. Prices are in cents: 2500 means 25.00.
        var peteBestseller = AddListing(db, shadypete, curiosities, "Haunted snow globe", 2500, 40);
        AddListing(db, shadypete, curiosities, "Map to nowhere in particular", 1200, 15);
        AddListing(db, shadypete, artifacts, "Museum gift shop mug (the real one)", 4500, 3);
        AddListing(db, shadypete, artifacts, "Slightly used crown", 99900, 1);
        AddListing(db, shadypete, electronics, "Pager, still receiving", 3000, 6);
        AddListing(db, shadypete, snacks, "Unmarked brownie", 800, 25);

        // 999 cents is the rounding example: 20% off is 199.8, rounded to 200, so 799.
        var soapBar = AddListing(db, grandmasoap, soap, "Lavender soap bar", 999, 60);
        AddListing(db, grandmasoap, soap, "Soap shaped like a smaller soap", 650, 30);
        AddListing(db, grandmasoap, soap, "Grandma's secret recipe (it is soap)", 1500, 12);
        AddListing(db, grandmasoap, snacks, "Cookies of uncertain origin", 450, 40);
        AddListing(db, grandmasoap, curiosities, "Knitted USB stick cosy", 700, 20);

        AddListing(db, oddjobs, electronics, "Cassette labelled 'DO NOT PLAY'", 1100, 4);
        AddListing(db, oddjobs, electronics, "Walkie-talkie, one only", 1800, 2);
        AddListing(db, oddjobs, electronics, "Calculator that only adds", 500, 9);
        AddListing(db, oddjobs, curiosities, "Jar of fog", 350, 50);
        AddListing(db, oddjobs, curiosities, "Left-handed spoon", 250, 18);
        AddListing(db, oddjobs, snacks, "Crisps, flavour classified", 300, 35);
        AddListing(db, oddjobs, artifacts, "Statue nobody will miss", 25000, 1);
        AddListing(db, oddjobs, snacks, "Mystery can", 400, 22);
        AddListing(db, oddjobs, soap, "Soap on a rope, rope sold separately", 900, 7);

        // Order history, inserted in bulk. These orders set up two rules:
        // 101 sales make shadypete featured, and 11 orders make loyalbuyer
        // eligible for the discount at grandmasoap.
        var history = new List<OrderRecord>();
        for (var i = 0; i < 101; i++)
            history.Add(CompletedOrder(bulkbuyer, shadypete, peteBestseller, 2500));
        for (var i = 0; i < 11; i++)
            history.Add(CompletedOrder(loyalbuyer, grandmasoap, soapBar, 999));
        db.BulkCopy(history);
    }

    private static int AddUser(AppDb db, string username) =>
        db.InsertWithInt32Identity(new UserRecord { Username = username });

    private static int AddCategory(AppDb db, string name) =>
        db.InsertWithInt32Identity(new CategoryRecord { Name = name });

    private static int AddListing(AppDb db, int vendorId, int categoryId, string title, long priceCents, int stock) =>
        db.InsertWithInt32Identity(new ListingRecord
        {
            VendorId = vendorId,
            CategoryId = categoryId,
            Title = title,
            Description = "Sold as seen. No refunds, no questions.",
            PriceCents = priceCents,
            Stock = stock,
        });

    private static OrderRecord CompletedOrder(int buyerId, int vendorId, int listingId, long priceCents) => new()
    {
        BuyerId = buyerId,
        VendorId = vendorId,
        ListingId = listingId,
        Quantity = 1,
        SubtotalCents = priceCents,
        DiscountCents = 0,
        TotalCents = priceCents,
        Status = OrderStatus.Completed,
    };
}