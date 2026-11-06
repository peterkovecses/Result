namespace Kovecses.Result.Sample.Controllers;

internal sealed class HttpCurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public string? UserId => httpContextAccessor.HttpContext?.Request.Headers["X-Customer-Id"].FirstOrDefault();
}
