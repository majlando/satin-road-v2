using LinqToDB.Data;

namespace SatinRoad.Api.Services;

/// <summary>
/// Fills the database at startup. The admin is always created. The demo data,
/// which sets up every rule so it can be shown straight away, is only added
/// when Seed:Demo is true and there are no listings yet. Every seeded user's
/// password is <see cref="Passwords.Demo"/>.
/// </summary>
public static class Seeder
{
    // Hashing is slow on purpose, so hash once and give every user the same hash.
    private static readonly Lazy<string> DemoHash = new(() => Passwords.Hash(Passwords.Demo));

    public static void Run(AppDb db, bool demo)
    {
        if (!db.Users.Any(u => u.Username == "admin"))
            db.Insert(new UserRecord { Username = "admin", Role = Roles.Admin, PasswordHash = DemoHash.Value });

        if (!demo || db.Listings.Any()) return;

        // Users, all named after famous criminals, real and fictional. Sellers sell; buyers buy.
        var vito       = AddUser(db, "vitocorleone");   // featured: 101 sales
        var walter     = AddUser(db, "walterwhite");    // tonymontana's favourite
        var capone     = AddUser(db, "alcapone");
        var pablo      = AddUser(db, "pabloescobar");
        var tommy      = AddUser(db, "tommyshelby");
        var blackbeard = AddUser(db, "blackbeard");
        var drevil     = AddUser(db, "drevil");
        var lex        = AddUser(db, "lexluthor");
        var tony       = AddUser(db, "tonymontana");    // bought vito's 101, and 11 orders at walterwhite
        AddUser(db, "jacksparrow");                     // fresh, for the raid demo
        AddUser(db, "bonnieparker");
        AddUser(db, "clydebarrow");
        AddUser(db, "jessejames");

        // Categories.
        var drugs     = AddCategory(db, "Hard Drugs");
        var booze     = AddCategory(db, "Bootleg Booze");
        var military  = AddCategory(db, "Military Hardware");
        var nuclear   = AddCategory(db, "Nuclear Materials");
        var treasure  = AddCategory(db, "Priceless Stolen Treasures");
        var cash      = AddCategory(db, "Counterfeit Cash");
        var documents = AddCategory(db, "Forged Documents");
        var vehicles  = AddCategory(db, "Stolen Vehicles");
        var animals   = AddCategory(db, "Exotic Contraband");
        var hitmen    = AddCategory(db, "Hitmen for Hire");
        var heists    = AddCategory(db, "Heist Crews");
        var lairs     = AddCategory(db, "Evil Lairs");

        // Listings. Prices are in cents: 2500 means 25.00.
        var vitoBestseller = AddListing(db, vito, military, "Tommy gun, violin case included", 2500, 40,
            "Classic 1920s styling. The violin does not play.");
        AddListing(db, vito, hitmen, "Semi-retired hitman, weekends only", 50000, 1,
            "Makes offers you can't refuse.");
        AddListing(db, vito, treasure, "The Mona Lisa (the real one, trust me)", 99900, 1,
            "The one in the Louvre is the fake. Smile slightly smug.");
        AddListing(db, vito, animals, "Horse head, delivered to your bed", 7500, 5,
            "For persuading film producers.");
        AddListing(db, vito, heists, "Crew for robbing three casinos at once", 1100000, 1,
            "Eleven professionals. Twelve if you count the one planning a sequel.");

        // 999 cents is the rounding example: 20% off is 199.8, rounded to 200, so 799.
        var blueCandy = AddListing(db, walter, drugs, "Suspiciously blue rock candy, 99.1% pure", 999, 60,
            "Chemistry teacher quality. Say my name.");
        AddListing(db, walter, drugs, "Mobile lab in a very old RV", 1800000, 1,
            "Slight smell. Do not ask about the barrel.");
        AddListing(db, walter, cash, "Barrel of cash, buried in the desert", 8000000, 1,
            "Coordinates included. Shovel not included.");
        AddListing(db, walter, documents, "New identity via vacuum cleaner repair shop", 150000, 2,
            "Ask for Saul. Relocate to Nebraska.");

        AddListing(db, capone, booze, "Bathtub full of bathtub gin", 4500, 12,
            "Prohibition vintage. Bathtub included.");
        AddListing(db, capone, booze, "Speakeasy, password protected", 900000, 1,
            "Fully stocked. The password is 'password'.");
        AddListing(db, capone, documents, "Tax return that adds up perfectly", 25000, 3,
            "Guaranteed to fool the IRS. Previous owner not satisfied.");
        AddListing(db, capone, vehicles, "Armoured Cadillac with bullet holes", 120000, 2,
            "The holes are for ventilation.");

        AddListing(db, pablo, drugs, "Cocaine mountain, 1 metric tonne", 9900000, 2,
            "Fresh from the jungle. Hippos sold separately.");
        AddListing(db, pablo, animals, "Pet hippo, slightly feral", 450000, 4,
            "Grew up at the ranch. Eats the lawn and the gardener.");
        AddListing(db, pablo, cash, "Pallet of rat-chewed dollars", 60000, 8,
            "Ten percent lost to rats each year. Still a bargain.");
        AddListing(db, pablo, heists, "Prison you built yourself, escape included", 3000000, 1,
            "Luxury cells. Comes with a football pitch.");

        AddListing(db, tommy, documents, "Licence to run a racecourse", 15000, 5,
            "By order of the Peaky Blinders.");
        AddListing(db, tommy, military, "Flat cap with razor blades sewn in", 3500, 20,
            "Fashionable and threatening.");
        AddListing(db, tommy, booze, "Crate of Irish whiskey, never taxed", 12000, 10,
            "Drink at 11 in the morning, like a professional.");
        AddListing(db, tommy, animals, "Smuggled racehorse, renamed twice", 220000, 1,
            "Wins every race it is drugged for.");

        AddListing(db, blackbeard, treasure, "Chest of Spanish gold doubloons", 500000, 3,
            "Cursed, but only a little.");
        AddListing(db, blackbeard, vehicles, "Pirate ship, cannons loaded", 2000000, 1,
            "Sails itself. Crew may mutiny.");
        AddListing(db, blackbeard, treasure, "Map where X marks the spot", 2000, 30,
            "Several X's. One of them is right.");
        AddListing(db, blackbeard, documents, "Letter of marque: piracy is now legal", 35000, 10,
            "Signed by a king, probably.");

        AddListing(db, drevil, nuclear, "Sharks with frickin' laser beams", 100000000, 1,
            "One hundred billion dollars! Or one million. Whatever you have.");
        AddListing(db, drevil, lairs, "Hollowed-out volcano, sea views", 50000000, 1,
            "Lava, monorail and a chair for stroking cats.");
        AddListing(db, drevil, heists, "Mini-me clone, 1/8th size", 80000, 2,
            "Does everything you do, but smaller.");
        AddListing(db, drevil, cash, "One-million-dollar bill, still wet", 1500, 50,
            "Printed this morning. Do not iron.");

        AddListing(db, lex, nuclear, "Kryptonite, glows in the dark", 1500000, 3,
            "Keep away from flying men in capes.");
        AddListing(db, lex, lairs, "Underground lair below Metropolis", 25000000, 1,
            "Good transport links. Bald-friendly lighting.");
        AddListing(db, lex, nuclear, "Plutonium for one time machine", 3000000, 4,
            "Enough for 1.21 gigawatts. The previous owners are very angry.");
        AddListing(db, lex, treasure, "British crown jewels, slightly worn", 300000, 1,
            "The ones in the Tower are replicas. Honest.");

        AddListing(db, tony, hitmen, "Say hello to my little friend", 30000, 2,
            "Grenade launcher with a strong personality.");
        AddListing(db, tony, military, "Soviet tank, one careful owner", 750000, 2,
            "Low mileage. Parking may be tricky.");
        AddListing(db, tony, vehicles, "Getaway car with the engine running", 40000, 6,
            "Never stopped. Fuel bill may be high.");

        // Order history, inserted in bulk. These orders set up two rules:
        // 101 sales make vitocorleone featured, and 11 orders make tonymontana
        // eligible for the discount at walterwhite.
        var history = new List<OrderRecord>();
        for (var i = 0; i < 101; i++)
            history.Add(CompletedOrder(tony, vito, vitoBestseller, 2500));
        for (var i = 0; i < 11; i++)
            history.Add(CompletedOrder(tony, walter, blueCandy, 999));
        db.BulkCopy(history);
    }

    private static int AddUser(AppDb db, string username) =>
        db.InsertWithInt32Identity(new UserRecord { Username = username, PasswordHash = DemoHash.Value });

    private static int AddCategory(AppDb db, string name) =>
        db.InsertWithInt32Identity(new CategoryRecord { Name = name });

    private static int AddListing(AppDb db, int vendorId, int categoryId, string title, long priceCents, int stock, string description) =>
        db.InsertWithInt32Identity(new ListingRecord
        {
            VendorId = vendorId,
            CategoryId = categoryId,
            Title = title,
            Description = description,
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