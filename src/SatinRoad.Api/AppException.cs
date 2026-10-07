namespace SatinRoad.Api;

/// <summary>
/// A rule violation with the HTTP status it should become. d.
/// </summary>
public class AppException(int status, string message) : Exception(message)
{
    public int Status { get; } = status;

    /// <summary>400: the request itself is wrong, and retrying it unchanged will not help.</summary>
    public static AppException BadRequest(string message) => new(400, message);

    /// <summary>401: no acting user, or one we do not recognise.</summary>
    public static AppException Unauthorized(string message) => new(401, message);

    /// <summary>403: we know who you are, and you may not do this.</summary>
    public static AppException Forbidden(string message) => new(403, message);

    /// <summary>
    /// 404: no such thing — also used instead of 403 for another user's listing,
    /// so the API does not reveal which ids exist.
    /// </summary>
    public static AppException NotFound(string what) => new(404, $"{what} not found");

    /// <summary>409: the request is valid but the current state refuses it.</summary>
    public static AppException Conflict(string message) => new(409, message);
}

/// <summary>
/// Maps to ProblemDetails. Anything else is
/// left alone, so a genuine bug still surfaces as a 500 rather than being
/// quietly reshaped into a tidy error.
/// </summary>
public class AppExceptionHandler(IProblemDetailsService problemDetails) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not AppException appException) return false;

        context.Response.StatusCode = appException.Status;

        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            ProblemDetails =
            {
                Status = appException.Status,
                Title = appException.Message,
            },
        });
    }
}