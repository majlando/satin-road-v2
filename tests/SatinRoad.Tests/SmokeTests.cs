using System.Net;

namespace SatinRoad.Tests;

/// <summary>Proves the real API boots inside a test.</summary>
public class SmokeTests
{
    [Fact]
    public async Task Health_endpoint_answers_ok()
    {
        await using var api = new ApiFactory();
        using var client = api.CreateClient();

        var response = await client.GetAsync("/health", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task OpenApi_document_is_served()
    {
        await using var api = new ApiFactory();
        using var client = api.CreateClient();

        var response = await client.GetAsync("/openapi/v1.json", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}