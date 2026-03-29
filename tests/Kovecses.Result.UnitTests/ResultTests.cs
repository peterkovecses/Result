namespace Kovecses.Result.UnitTests;

public class ResultTests
{
    [Fact]
    public void Success_WhenCalled_ShouldReturnSuccessResult()
    {
        // Act
        var result = Result.Success();

        // Assert
        result.Should().BeSuccess();
    }

    [Fact]
    public void SuccessGeneric_WhenCalled_ShouldReturnSuccessResultWithValue()
    {
        // Arrange
        const string value = "SuccessData";

        // Act
        var result = Result.Success(value);

        // Assert
        result.Should().BeSuccess().HaveData(value);
    }

    [Fact]
    public void Failure_WhenCalledWithError_ShouldReturnFailureResult()
    {
        // Arrange
        var error = Error.NotFound();

        // Act
        var result = Result.Failure(error);

        // Assert
        result.Should().BeFailure().HaveErrorCode(error.Code);
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
        result.Should().BeFailure().HaveErrorCode(code);
        result.Should().HaveError().HaveMessage(message);
    }

    [Theory]
    [InlineData(42)]
    [InlineData("SuccessData")]
    [InlineData(null)]
    public void ImplicitOperator_ShouldReturnSuccessResultWithCorrectData<T>(T value)
    {
        // Act
        Result<T> result = value;

        // Assert
        result.Should().BeSuccess().HaveData(value);
    }

    [Fact]
    public void ImplicitOperator_WhenAssigningListToIEnumerable_ShouldWorkCorrectly()
    {
        // Arrange
        var list = new List<int> { 1, 2, 3 };

        // Act
        Result<IEnumerable<int>> result = list;

        // Assert
        result.Should().BeSuccess().HaveData(list);
    }

    [Fact]
    public void ImplicitOperator_WhenAssigningErrorToResult_ShouldReturnFailure()
    {
        // Arrange
        var error = Error.NotFound();

        // Act
        Result result = error;

        // Assert
        result.Should().BeFailure().HaveErrorCode(error.Code);
    }

    [Fact]
    public void ImplicitOperator_WhenAssigningErrorToGenericResult_ShouldReturnFailure()
    {
        // Arrange
        var error = Error.Validation([]);

        // Act
        Result<int> result = error;

        // Assert
        result.Should().BeFailure().HaveErrorCode(error.Code);
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
        Assert.NotNull(deserialized);
        deserialized.Should().BeSuccess().HaveData(result.Data!);
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
        Assert.NotNull(deserialized);
        deserialized.Should().BeFailure().HaveErrorCode(error.Code);
        deserialized.Should().HaveError().HaveMetadata("Email", "Invalid");
    }
}
