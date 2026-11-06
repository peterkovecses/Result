using System.Text.Json;
using Kovecses.Result.FluentAssertions;

namespace Kovecses.Result.UnitTests;

public class FluentAssertionsTests
{
    [Fact]
    public void ResultAssertions_ShouldCoverNonGenericResultMembers()
    {
        var success = Result.Success(new Dictionary<string, object> { ["source"] = "catalog" });
        var failure = Result.Failure(
            [
                Error.Validation("Book.Isbn", "ISBN is required."),
                Error.Validation("Book.Title", "Title is required.")
            ]);

        success.Should().BeSuccess().HaveMetadata("source").HaveMetadata("source", "catalog");
        failure.Should()
            .BeFailure()
            .HaveErrors()
            .HaveCount(2)
            .AllBeOfType(ErrorType.Validation)
            .Contain(error => error.Code == "Book.Isbn")
            .And.BeFailure();
        failure.Should().HaveValidationErrorFor("Book.Isbn").Contain("required").ContainAll("ISBN").And.BeFailure();
    }

    [Fact]
    public void ResultAssertions_ShouldCoverGenericResultMembers()
    {
        var success = Result.Success("book-1", new Dictionary<string, object> { ["source"] = "catalog" });
        var failure = Result.Failure<string>(
            Error.Validation(ErrorCodes.Validation, "Invalid book.", new Dictionary<string, object>
            {
                ["Isbn"] = new[] { "ISBN is required.", "ISBN must have 13 digits." }
            }));

        success.Should()
            .BeSuccess()
            .HaveData("book-1")
            .HaveData(value => value!.StartsWith("book-", StringComparison.Ordinal))
            .HaveData(value => Assert.StartsWith("book-", value))
            .HaveMetadata("source", "catalog");
        Assert.Equal("book-1", success.Should().WhichData);

        failure.Should()
            .BeFailure()
            .HaveError(ErrorCodes.Validation)
            .HaveCode(ErrorCodes.Validation)
            .HaveMessage("Invalid book.")
            .HaveType(ErrorType.Validation)
            .HaveValidationProperty("Isbn")
            .Contain("required")
            .ContainAll("13 digits", "required")
            .HaveCount(2)
            .BeExactly("ISBN is required.", "ISBN must have 13 digits.")
            .And.HaveMetadata("Isbn", new[] { "ISBN is required.", "ISBN must have 13 digits." });

        failure.Should().HaveErrors().HaveCount(1).And.BeFailure();
        Result.Failure<string>(Error.Validation("Isbn", "ISBN is required."))
            .Should()
            .HaveValidationErrorFor("Isbn")
            .Contain("required")
            .And.BeFailure();
        failure.Should().ThrowOnValueAccess<InvalidOperationException>();
        failure.Should().ThrowOnValueAccess<ArgumentException>(errors => new ArgumentException(errors[0].Code));
    }

    [Fact]
    public void ErrorAssertions_ShouldCoverDirectErrorMembersAndValidationMetadata()
    {
        Error? error = Error.Validation(
            ErrorCodes.Validation,
            "Invalid book.",
            new Dictionary<string, object>
            {
                ["Isbn"] = new[] { "ISBN is required.", "ISBN must have 13 digits." }
            });

        error.Should()
            .NotBeNull()
            .HaveCode(ErrorCodes.Validation)
            .HaveMessage("Invalid book.")
            .HaveType(ErrorType.Validation)
            .HaveMetadata("Isbn")
            .HaveMetadata("Isbn", new[] { "ISBN is required.", "ISBN must have 13 digits." });

        var messages = error.Should().HaveValidationProperty("Isbn");
        messages.Contain("required").ContainAll("13 digits", "required").HaveCount(2).BeExactly(
            "ISBN is required.",
            "ISBN must have 13 digits.");
        Assert.Throws<InvalidOperationException>(() => _ = messages.And);

        var parentMessages = Result.Failure(error!).Should()
            .HaveError(ErrorCodes.Validation)
            .HaveValidationProperty("Isbn");
        parentMessages.Contain("required").And.HaveType(ErrorType.Validation);
    }

    [Fact]
    public void ValidationAssertions_ShouldExtractMessagesFromMetadataAndFlatErrors()
    {
        var error = Error.Validation(
            ErrorCodes.Validation,
            "Invalid book.",
            new Dictionary<string, object> { ["Isbn"] = new[] { "ISBN is required." } });

        var assertions = new ValidationAssertions<ResultAssertions>(
            ["ISBN is required.", "ISBN must have 13 digits."],
            Result.Success().Should());

        assertions.Contain("required").ContainAll("13 digits", "required").And.BeSuccess();
        Assert.Equal(
            new[] { "ISBN is required." },
            ValidationAssertions<ResultAssertions>.ExtractMessages(error, "Isbn"));

        var flatError = Error.Validation("Book.Isbn", "ISBN is required.");
        Assert.Equal(
            new[] { "ISBN is required." },
            ValidationAssertions<ResultAssertions>.ExtractMessages(flatError, "Book.Isbn"));
        Assert.Throws<ArgumentException>(() => ValidationAssertions<ResultAssertions>.ExtractMessages(error, "Title"));
    }

    [Fact]
    public void MetadataAssertions_ShouldSupportJsonDeserializedValues()
    {
        var error = JsonSerializer.Deserialize<Error>(
            """{"code":"General.Validation","message":"Invalid book.","type":2,"metadata":{"Isbn":"required","Count":2,"Available":true}}""");

        Assert.NotNull(error);
        error.Should().HaveMetadata("Isbn", "required");
        error.Should().HaveMetadata("Count", 2);
        error.Should().HaveMetadata("Available", true);
    }

    [Fact]
    public void ValidationPropertyAssertions_ShouldReadSupportedMetadataCollections()
    {
        using var document = JsonDocument.Parse("""{"messages":["ISBN is required.","ISBN must have 13 digits."]}""");
        var metadata = new Dictionary<string, object>
        {
            ["Array"] = new[] { "ISBN is required." },
            ["List"] = new List<string> { "ISBN is required." },
            ["Objects"] = new List<object> { "ISBN is required." },
            ["Enumerable"] = new HashSet<string> { "ISBN is required." },
            ["Json"] = document.RootElement.GetProperty("messages").Clone()
        };
        var error = Error.Validation(ErrorCodes.Validation, "Invalid book.", metadata);

        foreach (var field in metadata.Keys)
        {
            error.Should().HaveValidationProperty(field).Contain("required");
            Result.Failure(error).Should()
                .HaveError(ErrorCodes.Validation)
                .HaveValidationProperty(field)
                .Contain("required");
        }

        var unsupported = Error.Validation(
            ErrorCodes.Validation,
            "Invalid book.",
            new Dictionary<string, object> { ["Count"] = 1 });
        Assert.Throws<InvalidOperationException>(() => unsupported.Should().HaveValidationProperty("Count"));
    }

    [Fact]
    public void JsonMetadataAssertions_ShouldNormalizeJsonElementValues()
    {
        using var document = JsonDocument.Parse(
            """{"String":"book-1","Number":42,"True":true,"False":false,"Null":null,"Object":{"id":1}}""");
        var metadata = document.RootElement.EnumerateObject()
            .ToDictionary(property => property.Name, property => (object)property.Value.Clone());
        var error = Error.Validation(ErrorCodes.Validation, "Invalid book.", metadata);

        error.Should()
            .HaveMetadata("String", "book-1")
            .HaveMetadata("Number", 42)
            .HaveMetadata("True", true)
            .HaveMetadata("False", false)
            .HaveMetadata("Null", null)
            .HaveMetadata("Object", """{"id":1}""");
    }

    [Fact]
    public void ExtractMessages_ShouldReadJsonMetadataArrays()
    {
        using var document = JsonDocument.Parse("""{"messages":["ISBN is required."]}""");
        var error = Error.Validation(
            ErrorCodes.Validation,
            "Invalid book.",
            new Dictionary<string, object>
            {
                ["Isbn"] = document.RootElement.GetProperty("messages").Clone()
            });

        Assert.Equal(
            new[] { "ISBN is required." },
            ValidationAssertions<ResultAssertions>.ExtractMessages(error, "Isbn"));
    }
}
