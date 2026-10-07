namespace SatinRoad.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController(AppDb db, CurrentUser currentUser) : ControllerBase
{
    /// <summary>Everyone, for the "Acting as" dropdown.</summary>
    [HttpGet]
    public async Task<List<UserDto>> List()
    {
        var users = await db.Users.OrderBy(u => u.Username).ToListAsync();
        return users.Select(UserDto.From).ToList();
    }

    /// <summary>The user named in the X-User-Id header.</summary>
    [HttpGet("me")]
    public async Task<UserDto> Me() => UserDto.From(await currentUser.GetAsync());

    /// <summary>Anyone may create a new, normal user.</summary>
    [HttpPost]
    public async Task<ActionResult<UserDto>> Create(CreateUserRequest request)
    {
        var username = request.Username.Trim();
        if (username.Length == 0)
            throw AppException.BadRequest("Username is required.");

        // The column is COLLATE NOCASE, so this comparison ignores case.
        if (await db.Users.AnyAsync(u => u.Username == username))
            throw AppException.Conflict($"The username '{username}' is taken.");

        var user = new UserRecord { Username = username, Role = Roles.User };
        user.Id = await db.InsertWithInt32IdentityAsync(user);
        return Created($"/api/users/{user.Id}", UserDto.From(user));
    }
}