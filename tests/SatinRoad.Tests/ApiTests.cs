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