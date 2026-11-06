namespace Kovecses.Result.Sample.MinimalApi;

public sealed class HttpCurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public string? UserId => httpContextAccessor.HttpContext?.Request.Headers["X-Customer-Id"].ToString();
}
