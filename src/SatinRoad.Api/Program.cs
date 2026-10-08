using System.Text.Json.Serialization;
using LinqToDB.DataProvider.SQLite;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using LinqToDB.Extensions.DependencyInjection;
using LinqToDB.Extensions.Logging;
using Microsoft.Data.Sqlite;

var builder = WebApplication.CreateBuilder(args);

// Strict number handling keeps numbers typed as `number` (not `string`)
// in the TypeScript client that swagger-typescript-api generates.
// Set twice on purpose: the OpenAPI document is built from the minimal-API
// options, while controllers serialize with the MVC options. Setting only one
// would let the document and the actual wire behaviour disagree.
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.NumberHandling = JsonNumberHandling.Strict);
builder.Services.ConfigureHttpJsonOptions(o =>
    o.SerializerOptions.NumberHandling = JsonNumberHandling.Strict);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<AppExceptionHandler>();
builder.Services.AddHttpContextAccessor();

// Login sets this cookie. It is an API, so a request that is not allowed gets
// a 401 or 403 instead of the default redirect to a login page.
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.Cookie.Name = "satinroad.auth";
        o.Cookie.HttpOnly = true;
        o.Cookie.SameSite = SameSiteMode.Lax;
        o.ExpireTimeSpan = TimeSpan.FromDays(7);
        o.SlidingExpiration = true;
        o.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
        o.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    });

// The keys that sign the login cookie. Without a folder they live in memory,
// so every restart would log everyone out. Docker points this at the data volume.
var keysPath = builder.Configuration["DataProtection:KeysPath"];
if (!string.IsNullOrWhiteSpace(keysPath))
    builder.Services.AddDataProtection()
        .SetApplicationName("SatinRoad")
        .PersistKeysToFileSystem(new DirectoryInfo(keysPath));

builder.Services.AddLinqToDBContext<AppDb>((provider, options) =>
    options
        .UseSQLite(BuildConnectionString(builder.Configuration), SQLiteProvider.Microsoft)
        .UseDefaultLogging(provider));

builder.Services.AddScoped<CurrentUser>();
builder.Services.AddScoped<Catalog>();
builder.Services.AddSingleton<IRaidRoller, RandomRaidRoller>();
builder.Services.AddSingleton(services =>
    new RaidPolicy(services.GetRequiredService<IConfiguration>().GetValue("Fbi:RaidChance", 0.01)));
builder.Services.AddScoped<PurchaseService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDb>();
    Schema.Ensure(db);
    Seeder.Run(db, app.Configuration.GetValue("Seed:Demo", false));
}

app.UseExceptionHandler();

app.MapOpenApi();                                                            // /openapi/v1.json
app.UseSwaggerUI(o => o.SwaggerEndpoint("/openapi/v1.json", "Satin Road"));  // /swagger

app.UseAuthentication();

app.MapControllers();

// Liveness probe for the smoke test and for checking by hand. Docker Compose
// does not probe it: the aspnet image ships no curl or wget to call it with.
app.MapGet("/health", () => Results.Ok(new { status = "ok" }))
   .WithName("GetHealth")
   .WithTags("Health");

app.Run();

/// <summary>
/// Forces <c>Foreign Keys=True</c> on whatever connection string is configured.
/// SQLite ignores REFERENCES clauses unless the pragma is on per connection, and
/// setting it here means no environment can accidentally run without it.
/// </summary>
static string BuildConnectionString(IConfiguration configuration)
{
    var configured = configuration.GetConnectionString("Default") ?? "Data Source=satinroad.db";
    return new SqliteConnectionStringBuilder(configured) { ForeignKeys = true }.ToString();
}

/// <summary>Named so <c>WebApplicationFactory&lt;Program&gt;</c> can boot the real API in tests.</summary>
public partial class Program;