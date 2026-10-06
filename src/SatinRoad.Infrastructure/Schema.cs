using System.Reflection;

namespace SatinRoad.Infrastructure;

/// <summary>
/// Applies schema.sql. Every statement in it is <c>IF NOT EXISTS</c>, so running
/// this on every startup is safe and idempotent — it replaces migrations.
/// </summary>
public static class Schema
{
    private const string ResourceName = "SatinRoad.Infrastructure.schema.sql";

    public static void Ensure(AppDb db) => db.Execute(ReadScript());

    /// <summary>The schema script, as embedded in this assembly.</summary>
    public static string ReadScript()
    {
        var assembly = typeof(Schema).Assembly;

        using var stream = assembly.GetManifestResourceStream(ResourceName)
                           ?? throw new InvalidOperationException(
                               $"Embedded resource '{ResourceName}' is missing. Available: " +
                               $"{string.Join(", ", assembly.GetManifestResourceNames())}. " +
                               "Check the EmbeddedResource item in SatinRoad.Infrastructure.csproj.");

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}