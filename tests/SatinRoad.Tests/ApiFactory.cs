using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace SatinRoad.Tests;

/// <summary>
/// The real API, running inside the test, on its own throwaway SQLite file.
/// Each test makes its own, so no test can see another test's data.
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>
{
    private readonly string _file = Path.Combine(Path.GetTempPath(), $"satin-api-{Guid.NewGuid():N}.db");

    /// <summary>
    /// What the raid roller returns. 0.99 never raids; set it to 0 to force a raid.
    /// </summary>
    public double NextRoll { get; set; } = 0.99;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(config => config.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Default"] = $"Data Source={_file}",
            ["Seed:Demo"] = "false",
        }));

        // Registered last, so it replaces the random roller.
        builder.ConfigureTestServices(services =>
            services.AddSingleton<IRaidRoller>(new FuncRoller(() => NextRoll)));
    }

    /// <summary>
    /// Every user added by <see cref="AddUser"/> has the password <see cref="Passwords.Demo"/>.
    /// Hashing is slow on purpose, so it is done once for all tests.
    /// </summary>
    private static readonly Lazy<string> DemoHash = new(() => Passwords.Hash(Passwords.Demo));

    /// <summary>A client logged in as the given user. It keeps the auth cookie between requests.</summary>
    public HttpClient As(int userId)
    {
        var username = Db(db => db.Users.Single(u => u.Id == userId).Username);
        var client = CreateClient();
        var response = client.PostAsJsonAsync("/api/auth/login", new LoginRequest(username, Passwords.Demo))
            .GetAwaiter().GetResult();
        response.EnsureSuccessStatusCode();
        return client;
    }

    /// <summary>Runs code directly against the database, to set up or check data.</summary>
    public T Db<T>(Func<AppDb, T> work)
    {
        using var scope = Services.CreateScope();
        return work(scope.ServiceProvider.GetRequiredService<AppDb>());
    }

    public int AddUser(string username, string role = Roles.User) =>
        Db(db => db.InsertWithInt32Identity(new UserRecord { Username = username, Role = role, PasswordHash = DemoHash.Value }));

    public int AddCategory(string name) =>
        Db(db => db.InsertWithInt32Identity(new CategoryRecord { Name = name }));

    public int AddListing(int vendorId, int categoryId, long priceCents = 1_000, int stock = 5) =>
        Db(db => db.InsertWithInt32Identity(new ListingRecord
        {
            VendorId = vendorId,
            CategoryId = categoryId,
            Title = "Something suspicious",
            Description = "No questions asked",
            PriceCents = priceCents,
            Stock = stock,
        }));

    /// <summary>Adds finished orders directly, to build up a history.</summary>
    public void AddOrders(int count, int buyerId, int vendorId, int listingId, string status = OrderStatus.Completed) =>
        Db(db =>
        {
            for (var i = 0; i < count; i++)
                db.Insert(new OrderRecord
                {
                    BuyerId = buyerId, VendorId = vendorId, ListingId = listingId,
                    Quantity = 1, SubtotalCents = 100, DiscountCents = 0, TotalCents = 100,
                    Status = status,
                });
            return 0;
        });

    public int StockOf(int listingId) => Db(db => db.Listings.Single(l => l.Id == listingId).Stock);

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();

        // The last request can still be handing its connection back after its
        // response arrived, and Windows will not delete a file that is open.
        // So: empty the connection pool and try again for up to a second.
        for (var attempt = 1; ; attempt++)
        {
            SqliteConnection.ClearAllPools();
            try
            {
                File.Delete(_file);
                return;
            }
            catch (IOException) when (attempt < 20)
            {
                await Task.Delay(50);
            }
        }
    }
}