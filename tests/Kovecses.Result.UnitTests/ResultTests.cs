namespace Kovecses.Result.UnitTests;

public class ResultTests
{
    [Fact]
    public void Success_WhenCalled_ShouldReturnSuccessResult()
    {
        // Act
        var result = Result.Success();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.IsFailure.ShouldBeFalse();
        result.Error.ShouldBeNull();
    }

    [Fact]
    public void SuccessGeneric_WhenCalled_ShouldReturnSuccessResultWithValue()
    {
        // Arrange
        const string value = "SuccessData";

        // Act
        var result = Result.Success(value);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldBe(value);
    }

    [Fact]
    public void Failure_WhenCalledWithError_ShouldReturnFailureResult()
    {
        // Arrange
        var error = Error.NotFound();

        // Act
        var result = Result.Failure(error);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(error);
    }

    [Fact]
    public void Failure_WhenCalledWithCodeAndMessage_ShouldReturnFailureResult()
    {
        // Arrange
        const string code = "Test.Code";
        const string message = "Test Message";

        // Act
        var result = Result.Failure(code, message);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldNotBeNull();
        result.Error.Code.ShouldBe(code);
        result.Error.Message.ShouldBe(message);
    }

    [Fact]
    public void ImplicitOperator_WhenAssigningValue_ShouldReturnSuccessResult()
    {
        // Arrange
        const int value = 42;

        // Act
        Result<int> result = value;

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldBe(value);
    }

    [Fact]
    public void Serialization_WhenSuccess_ShouldSerializeAndDeserializeCorrectly()
    {
        // Arrange
        var result = Result.Success("TestContent");
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        // Act
        var json = JsonSerializer.Serialize(result, options);
        var deserialized = JsonSerializer.Deserialize<Result<string>>(json, options);

        // Assert
        deserialized.ShouldNotBeNull();
        deserialized.IsSuccess.ShouldBeTrue();
        deserialized.Data.ShouldBe(result.Data);
    }

    [Fact]
    public void Serialization_WhenFailure_ShouldSerializeAndDeserializeCorrectly()
    {
        // Arrange
        var error = Error.Validation(new Dictionary<string, object> { { "Email", "Invalid" } });
        var result = Result.Failure<string>(error);
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        // Act
        var json = JsonSerializer.Serialize(result, options);
        var deserialized = JsonSerializer.Deserialize<Result<string>>(json, options);

        // Assert
        deserialized.ShouldNotBeNull();
        deserialized.IsFailure.ShouldBeTrue();
        deserialized.Error.ShouldNotBeNull();
        deserialized.Error.Code.ShouldBe(error.Code);
        deserialized.Error.Metadata.ShouldNotBeNull();
        deserialized.Error.Metadata.ContainsKey("Email").ShouldBeTrue();
    }
}
