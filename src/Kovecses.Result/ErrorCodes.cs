namespace Kovecses.Result;

/// <summary>
/// Provides a set of standard error codes for general error scenarios.
/// </summary>
public static class ErrorCodes
{
    /// <summary>
    /// A general failure error code.
    /// </summary>
    public const string Failure = "General.Failure";

    /// <summary>
    /// An unexpected internal error code.
    /// </summary>
    public const string Unexpected = "General.Unexpected";

    /// <summary>
    /// A validation error code.
    /// </summary>
    public const string Validation = "General.Validation";

    /// <summary>
    /// A conflict error code.
    /// </summary>
    public const string Conflict = "General.Conflict";

    /// <summary>
    /// A resource not found error code.
    /// </summary>
    public const string NotFound = "General.NotFound";

    /// <summary>
    /// An unauthorized access error code.
    /// </summary>
    public const string Unauthorized = "General.Unauthorized";

    /// <summary>
    /// A forbidden access error code.
    /// </summary>
    public const string Forbidden = "General.Forbidden";

    /// <summary>
    /// A timeout error code.
    /// </summary>
    public const string Timeout = "General.Timeout";

    /// <summary>
    /// A canceled operation error code.
    /// </summary>
    public const string Canceled = "General.Canceled";
}
