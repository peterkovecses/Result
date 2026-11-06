namespace Kovecses.Result.Sample.BookStore.Core.Application;

/// <summary>
/// The caller of the current request. Implemented by the host (for example from the HTTP context).
/// </summary>
public interface ICurrentUser
{
    /// <summary>The authenticated user's id, or <c>null</c> for an anonymous caller.</summary>
    string? UserId { get; }
}

public static class CurrentUserExtensions
{
    /// <summary>
    /// Returns the user id, or an Unauthorized failure when nobody is signed in.
    /// </summary>
    public static Result<string> RequireUserId(this ICurrentUser currentUser)
        => currentUser.UserId is { Length: > 0 } userId
            ? userId
            : Error.Unauthorized(message: "You must be signed in to perform this action.");
}
