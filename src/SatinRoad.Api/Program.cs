using System.Text.Json.Serialization;
using LinqToDB.DataProvider.SQLite;
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

builder.Services.AddLinqToDBContext<AppDb>((provider, options) =>
    options
        .UseSQLite(BuildConnectionString(builder.Configuration), SQLiteProvider.Microsoft)
        .UseDefaultLogging(provider));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDb>();
    Schema.Ensure(db);
}

app.UseExceptionHandler();

app.MapOpenApi();                                                            // /openapi/v1.json
app.UseSwaggerUI(o => o.SwaggerEndpoint("/openapi/v1.json", "Satin Road"));  // /swagger

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