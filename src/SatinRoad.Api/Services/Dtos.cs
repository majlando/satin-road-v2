namespace SatinRoad.Api;

// The shapes the API sends and receives. Kept apart from the database records,
// so a column can change without changing what the browser sees.

public record UserDto(int Id, string Username, string Role, bool IsSeized)
{
    public static UserDto From(UserRecord u) => new(u.Id, u.Username, u.Role, u.IsSeized);
}

public record CreateUserRequest(string Username);