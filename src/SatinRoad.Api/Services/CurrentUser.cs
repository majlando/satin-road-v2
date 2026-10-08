using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace SatinRoad.Api.Services;

/// <summary>
/// Who is making this request. Logging in sets an auth cookie holding the user's
/// id; this class reads it and looks that user up, fresh from the database, so a
/// raid or a role change takes effect on the next request.
/// </summary>
public class CurrentUser(IHttpContextAccessor http, AppDb db)
{
    private UserRecord? _user;

    /// <summary>The logged-in user. 401 if nobody is logged in.</summary>
    public async Task<UserRecord> GetAsync()
    {
        if (_user is not null) return _user;

        var raw = http.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(raw, out var id))
            throw AppException.Unauthorized("Log in first.");

        _user = await db.Users.FirstOrDefaultAsync(u => u.Id == id)
                ?? throw AppException.Unauthorized("Log in first.");
        return _user;
    }

    /// <summary>The logged-in user, who must be an admin. 403 otherwise.</summary>
    public async Task<UserRecord> RequireAdminAsync()
    {
        var user = await GetAsync();
        if (user.Role != Roles.Admin)
            throw AppException.Forbidden("Only an admin can do this.");
        return user;
    }

    /// <summary>The logged-in user, who must not have been shut down by the FBI. 403 otherwise.</summary>
    public async Task<UserRecord> RequireActiveAsync()
    {
        var user = await GetAsync();
        if (user.IsSeized)
            throw AppException.Forbidden("Your shop was shut down by the FBI.");
        return user;
    }

    /// <summary>Logs the user in: the response sets the auth cookie.</summary>
    public async Task SignInAsync(UserRecord user)
    {
        var identity = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
            ],
            CookieAuthenticationDefaults.AuthenticationScheme);

        await Context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
        _user = user;
    }

    /// <summary>Logs out: the response clears the auth cookie.</summary>
    public async Task SignOutAsync()
    {
        await Context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        _user = null;
    }

    private HttpContext Context =>
        http.HttpContext ?? throw new InvalidOperationException("No request to sign in or out of.");
}
