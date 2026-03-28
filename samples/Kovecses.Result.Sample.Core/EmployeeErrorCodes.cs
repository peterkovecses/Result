namespace Kovecses.Result.Sample.Core;

/// <summary>
/// Provides a set of custom error codes specific to the Employee domain logic.
/// </summary>
public static class EmployeeErrorCodes
{
    public const string NameTooShort = "Employee.NameTooShort";
    public const string CannotDeleteBoss = "Employee.CannotDeleteBoss";
}
