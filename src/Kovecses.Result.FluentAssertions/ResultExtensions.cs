namespace Kovecses.Result.FluentAssertions;

/// <summary>
/// Provides extension methods to start fluent assertions on <see cref="Result"/> types.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Starts fluent assertions for the specified <see cref="Result"/>.
    /// </summary>
    /// <param name="instance">The result instance to assert on.</param>
    /// <returns>A <see cref="ResultAssertions"/> object.</returns>
    public static ResultAssertions Should(this Result instance) => new(instance);

    /// <summary>
    /// Starts fluent assertions for the specified <see cref="Result{TData}"/>.
    /// </summary>
    /// <typeparam name="TData">The type of the data.</typeparam>
    /// <param name="instance">The result instance to assert on.</param>
    /// <returns>A <see cref="ResultAssertions{TData}"/> object.</returns>
    public static ResultAssertions<TData> Should<TData>(this Result<TData> instance) => new(instance);

    /// <summary>
    /// Starts fluent assertions for the specified <see cref="Error"/>.
    /// </summary>
    /// <param name="instance">The error instance to assert on.</param>
    /// <returns>An <see cref="ErrorAssertions"/> object.</returns>
    public static ErrorAssertions Should(this Error? instance) => new(instance);
}
