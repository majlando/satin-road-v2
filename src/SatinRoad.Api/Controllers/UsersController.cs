namespace SatinRoad.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController(AppDb db, CurrentUser currentUser) : ControllerBase
{
    /// <summary>Everyone.</summary>
    [HttpGet]
    public async Task<List<UserDto>> List()
    {
        var users = await db.Users.OrderBy(u => u.Username).ToListAsync();
        return users.Select(UserDto.From).ToList();
    }

    /// <summary>The logged-in user. 401 if nobody is.</summary>
    [HttpGet("me")]
    public async Task<UserDto> Me() => UserDto.From(await currentUser.GetAsync());

    /// <summary>Anyone may register a new, normal user, who is then logged in.</summary>
    [HttpPost]
    public async Task<ActionResult<UserDto>> Create(CreateUserRequest request)
    {
        var username = request.Username.Trim();
        if (username.Length == 0)
            throw AppException.BadRequest("Username is required.");
        if (request.Password.Length < Passwords.MinLength)
            throw AppException.BadRequest($"The password must be at least {Passwords.MinLength} characters.");

        // The column is COLLATE NOCASE, so this comparison ignores case.
        if (await db.Users.AnyAsync(u => u.Username == username))
            throw AppException.Conflict($"The username '{username}' is taken.");

        var user = new UserRecord { Username = username, Role = Roles.User, PasswordHash = Passwords.Hash(request.Password) };
        user.Id = await db.InsertWithInt32IdentityAsync(user);
        await currentUser.SignInAsync(user);
        return Created($"/api/users/{user.Id}", UserDto.From(user));
    }
}