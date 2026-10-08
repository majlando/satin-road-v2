namespace SatinRoad.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(AppDb db, CurrentUser currentUser) : ControllerBase
{
    /// <summary>Checks the password and sets the auth cookie.</summary>
    [HttpPost("login")]
    public async Task<UserDto> Login(LoginRequest request)
    {
        // The column is COLLATE NOCASE, so the username ignores case.
        var username = request.Username.Trim();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Username == username);

        // One message for both mistakes, so it does not reveal which usernames exist.
        if (user is null || !Passwords.Verify(user, request.Password))
            throw AppException.Unauthorized("Wrong username or password.");

        await currentUser.SignInAsync(user);
        return UserDto.From(user);
    }

    /// <summary>Clears the auth cookie. Fine to call when not logged in.</summary>
    [HttpPost("logout")]
    public async Task<NoContentResult> Logout()
    {
        await currentUser.SignOutAsync();
        return NoContent();
    }
}
