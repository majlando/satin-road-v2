namespace SatinRoad.Api.Services;

/// <summary>
/// Who is making this request. There is no login: the browser sends the chosen
/// user's id in the X-User-Id header, and this class looks that user up.
/// </summary>
public class CurrentUser(IHttpContextAccessor http, AppDb db)
{
    public const string Header = "X-User-Id";

    private UserRecord? _user;

    /// <summary>The acting user. 401 if the header is missing or names nobody.</summary>
    public async Task<UserRecord> GetAsync()
    {
        if (_user is not null) return _user;

        var raw = http.HttpContext?.Request.Headers[Header].ToString();
        if (!int.TryParse(raw, out var id))
            throw AppException.Unauthorized("Choose who you are acting as first.");

        _user = await db.Users.FirstOrDefaultAsync(u => u.Id == id)
                ?? throw AppException.Unauthorized($"There is no user with id {id}.");
        return _user;
    }

    /// <summary>The acting user, who must be an admin. 403 otherwise.</summary>
    public async Task<UserRecord> RequireAdminAsync()
    {
        var user = await GetAsync();
        if (user.Role != Roles.Admin)
            throw AppException.Forbidden("Only an admin can do this.");
        return user;
    }

    /// <summary>The acting user, who must not have been shut down by the FBI. 403 otherwise.</summary>
    public async Task<UserRecord> RequireActiveAsync()
    {
        var user = await GetAsync();
        if (user.IsSeized)
            throw AppException.Forbidden("Your shop was shut down by the FBI.");
        return user;
    }
}