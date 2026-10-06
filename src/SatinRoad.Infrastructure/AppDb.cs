namespace SatinRoad.Infrastructure;

/// <summary>
/// The linq2db data context. Registered per request with AddLinqToDBContext,
/// so a single request shares one connection and can wrap work in a transaction.
/// </summary>
public class AppDb(DataOptions<AppDb> options) : DataConnection(options.Options)
{
    public ITable<UserRecord> Users => this.GetTable<UserRecord>();
    public ITable<CategoryRecord> Categories => this.GetTable<CategoryRecord>();
    public ITable<ListingRecord> Listings => this.GetTable<ListingRecord>();
    public ITable<OrderRecord> Orders => this.GetTable<OrderRecord>();
}