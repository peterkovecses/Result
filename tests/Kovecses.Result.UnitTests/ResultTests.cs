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

    [Fact]
    public void Match_WhenSuccess_ShouldExecuteOnSuccess()
    {
        // Arrange
        var result = Result.Success();

        // Act
        var output = result.Match(() => "Success", error => "Failure");

        // Assert
        Assert.Equal("Success", output);
    }

    [Fact]
    public void Match_WhenFailure_ShouldExecuteOnFailure()
    {
        // Arrange
        var result = Result.Failure(Error.NotFound());

        // Act
        var output = result.Match(() => "Success", error => "Failure");

        // Assert
        Assert.Equal("Failure", output);
    }

    [Fact]
    public async Task MatchAsync_WhenSuccess_ShouldExecuteOnSuccess()
    {
        // Arrange
        var result = Result.Success();

        // Act
        var output = await result.MatchAsync(() => Task.FromResult("Success"), error => Task.FromResult("Failure"));

        // Assert
        Assert.Equal("Success", output);
    }

    [Fact]
    public void MatchGeneric_WhenSuccess_ShouldExecuteOnSuccessWithData()
    {
        // Arrange
        var result = Result.Success("Data");

        // Act
        var output = result.Match(data => data, error => "Failure");

        // Assert
        Assert.Equal("Data", output);
    }

    [Fact]
    public void Map_WhenSuccess_ShouldTransformData()
    {
        // Arrange
        var result = Result.Success(10);

        // Act
        var mapped = result.Map(x => x * 2);

        // Assert
        mapped.Should().BeSuccess().HaveData(20);
    }

    [Fact]
    public void Map_WhenFailure_ShouldReturnFailure()
    {
        // Arrange
        var result = Result.Failure<int>(Error.Validation([]));

        // Act
        var mapped = result.Map(x => x * 2);

        // Assert
        mapped.Should().BeFailure();
    }

    [Fact]
    public async Task MapAsync_WhenSuccess_ShouldTransformData()
    {
        // Arrange
        var result = Result.Success(10);

        // Act
        var mapped = await result.MapAsync(x => Task.FromResult(x * 2));

        // Assert
        mapped.Should().BeSuccess().HaveData(20);
    }

    [Fact]
    public void Bind_WhenSuccess_ShouldChainOperation()
    {
        // Arrange
        var result = Result.Success(10);

        // Act
        var bound = result.Bind(x => Result.Success(x.ToString()));

        // Assert
        bound.Should().BeSuccess().HaveData("10");
    }

    [Fact]
    public void Bind_WhenFailure_ShouldReturnFailure()
    {
        // Arrange
        var result = Result.Failure<int>(Error.NotFound());

        // Act
        var bound = result.Bind(x => Result.Success(x.ToString()));

        // Assert
        bound.Should().BeFailure();
    }

    [Fact]
    public async Task BindAsync_WhenSuccess_ShouldChainOperation()
    {
        // Arrange
        var result = Result.Success(10);

        // Act
        var bound = await result.BindAsync(x => Task.FromResult(Result.Success(x.ToString())));

        // Assert
        bound.Should().BeSuccess().HaveData("10");
    }

    [Fact]
    public void Combine_WhenAllSuccess_ShouldReturnSuccess()
    {
        // Act
        var result = Result.Combine(Result.Success(), Result.Success<int>(10));

        // Assert
        result.Should().BeSuccess();
    }

    [Fact]
    public void Combine_WhenOneFailure_ShouldReturnThatFailure()
    {
        // Arrange
        var error = Error.NotFound();
        
        // Act
        var result = Result.Combine(Result.Success(), Result.Failure(error));

        // Assert
        result.Should().BeFailure().HaveErrorCode(error.Code);
    }

    [Fact]
    public void Combine_WhenMultipleFailures_ShouldAggregateErrors()
    {
        // Arrange
        var error1 = Error.Validation(new Dictionary<string, object> { { "F1", "E1" } }, code: "V.1");
        var error2 = Error.Failure("F2", "C.2");

        // Act
        var result = Result.Combine(Result.Failure(error1), Result.Failure(error2));

        // Assert
        result.Should().BeFailure().HaveErrorCode(ErrorCodes.Validation);
        result.Should().HaveError().HaveMetadata("V.1");
        result.Should().HaveError().HaveMetadata("C.2");
    }
}
