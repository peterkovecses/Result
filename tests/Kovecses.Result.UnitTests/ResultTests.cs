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
        result.Should().BeFailure().HaveError(error.Code);
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
        result.Should().BeFailure().HaveError(code).HaveMessage(message);
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
    public void ImplicitOperator_WhenAssigningDerivedListToBaseInterface_ShouldSucceed()
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
        result.Should().BeFailure().HaveError(error.Code);
    }

    [Fact]
    public void ImplicitOperator_WhenAssigningErrorToGenericResult_ShouldReturnFailure()
    {
        // Arrange
        var error = Error.Validation("Validation.Error", "One or more validation errors occurred.");

        // Act
        Result<int> result = error;

        // Assert
        result.Should().BeFailure().HaveError(error.Code);
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
        var error = Error.Validation("V.1", "Message") with { Metadata = new Dictionary<string, object> { { "Email", "Invalid" } } };
        var result = Result.Failure<string>(error);
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        // Act
        var json = JsonSerializer.Serialize(result, options);
        var deserialized = JsonSerializer.Deserialize<Result<string>>(json, options);

        // Assert
        Assert.NotNull(deserialized);
        deserialized.Should().BeFailure().HaveError(error.Code).HaveMetadata("Email", "Invalid");
    }

    [Fact]
    public void Match_WhenSuccess_ShouldExecuteOnSuccess()
    {
        // Arrange
        var result = Result.Success();

        // Act
        var output = result.Match(() => "Success", errors => "Failure");

        // Assert
        Assert.Equal("Success", output);
    }

    [Fact]
    public void Match_WhenFailure_ShouldExecuteOnFailure()
    {
        // Arrange
        var result = Result.Failure(Error.NotFound());

        // Act
        var output = result.Match(() => "Success", errors => "Failure");

        // Assert
        Assert.Equal("Failure", output);
    }

    [Fact]
    public async Task MatchAsync_WhenSuccess_ShouldExecuteOnSuccess()
    {
        // Arrange
        var result = Result.Success();

        // Act
        var output = await result.MatchAsync(() => Task.FromResult("Success"), errors => Task.FromResult("Failure"));

        // Assert
        Assert.Equal("Success", output);
    }

    [Fact]
    public void MatchGeneric_WhenSuccess_ShouldExecuteOnSuccessWithData()
    {
        // Arrange
        var result = Result.Success("Data");

        // Act
        var output = result.Match(data => data, errors => "Failure");

        // Assert
        Assert.Equal("Data", output);
    }

    [Fact]
    public async Task MatchAsync_Result_WhenSuccess_ShouldExecuteOnSuccess()
    {
        // Arrange
        var result = Result.Success();

        // Act
        var output = await result.MatchAsync(() => Task.FromResult("Success"), errors => Task.FromResult("Failure"));

        // Assert
        Assert.Equal("Success", output);
    }

    [Fact]
    public async Task MatchAsync_ResultGeneric_WhenSuccess_ShouldExecuteOnSuccess()
    {
        // Arrange
        var result = Result.Success(10);

        // Act
        var output = await result.MatchAsync(
            data => Task.FromResult(data.ToString()), 
            errors => Task.FromResult("Failure"));

        // Assert
        Assert.Equal("10", output);
    }

    [Fact]
    public void Bind_Result_WhenSuccess_ShouldExecuteNext()
    {
        // Arrange
        var result = Result.Success();

        // Act
        var bound = result.Bind(() => Result.Success());

        // Assert
        bound.Should().BeSuccess();
    }

    [Fact]
    public void Bind_Result_WhenFailure_ShouldReturnFailure()
    {
        // Arrange
        var result = Result.Failure(Error.NotFound());

        // Act
        var bound = result.Bind(() => Result.Success());

        // Assert
        bound.Should().BeFailure();
    }

    [Fact]
    public async Task BindAsync_Result_WhenSuccess_ShouldExecuteNext()
    {
        // Arrange
        var result = Result.Success();

        // Act
        var bound = await result.BindAsync(() => Task.FromResult(Result.Success()));

        // Assert
        bound.Should().BeSuccess();
    }

    [Fact]
    public void Tap_WhenSuccess_ShouldExecuteAction()
    {
        // Arrange
        var result = Result.Success();
        var executed = false;

        // Act
        var tapped = result.Tap(() => executed = true);

        // Assert
        Assert.True(executed);
        tapped.Should().BeSuccess();
    }

    [Fact]
    public async Task TapAsync_WhenSuccess_ShouldExecuteFunc()
    {
        // Arrange
        var result = Result.Success();
        var executed = false;

        // Act
        var tapped = await result.TapAsync(() => { executed = true; return Task.CompletedTask; });

        // Assert
        Assert.True(executed);
        tapped.Should().BeSuccess();
    }

    [Fact]
    public void TapGeneric_WhenSuccess_ShouldExecuteActionWithData()
    {
        // Arrange
        var result = Result.Success(10);
        var value = 0;

        // Act
        var tapped = result.Tap(data => value = data);

        // Assert
        Assert.Equal(10, value);
        tapped.Should().BeSuccess().HaveData(10);
    }

    [Fact]
    public async Task TapAsyncGeneric_WhenSuccess_ShouldExecuteFuncWithData()
    {
        // Arrange
        var result = Result.Success(10);
        var value = 0;

        // Act
        var tapped = await result.TapAsync(data => { value = data; return Task.CompletedTask; });

        // Assert
        Assert.Equal(10, value);
        tapped.Should().BeSuccess().HaveData(10);
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
        var result = Result.Failure<int>(Error.Validation("Validation.Error", "Message"));

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
        result.Should().BeFailure().HaveError(error.Code);
    }

    [Fact]
    public void Combine_WhenMultipleFailures_ShouldAggregateErrorsAndMergeMetadata()
    {
        // Arrange
        var error1 = Error.Validation(ErrorCodes.Validation, "Message") with { Metadata = new Dictionary<string, object> { { "F1", "E1" } } };
        var error2 = Error.Failure("F2", "C.2") with { Metadata = new Dictionary<string, object> { { "C.2", "F2" } } };

        // Act
        var result = Result.Combine(Result.Failure(error1), Result.Failure(error2));

        // Assert
        result.Should().BeFailure().HaveError(ErrorCodes.Validation).HaveMetadata("F1", "E1"); // From validation error's metadata
        result.Should().HaveError("C.2").HaveMetadata("C.2", "F2"); // From failure error's code/message
    }

    [Fact]
    public void ValueOrThrow_WhenSuccess_ShouldReturnData()
    {
        // Arrange
        var result = Result.Success("SuccessData");

        // Act
        var value = result.ValueOrThrow();

        // Assert
        Assert.Equal("SuccessData", value);
    }

    [Fact]
    public void ValueOrThrow_WhenFailure_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var result = Result.Failure<string>(Error.NotFound());

        // Act & Assert
        result.Should().ThrowOnValueAccess<InvalidOperationException>();
    }

    [Fact]
    public void ValueOrThrow_WhenFailureWithCustomFactory_ShouldThrowCustomException()
    {
        // Arrange
        var result = Result.Failure<string>(Error.NotFound());

        // Act & Assert
        result.Should().ThrowOnValueAccess<ArgumentException>(errors => new ArgumentException(errors[0].Code));
    }

    [Fact]
    public void ValueOrDefault_WhenSuccess_ShouldReturnData()
    {
        // Arrange
        var result = Result.Success("SuccessData");

        // Act
        var value = result.ValueOrDefault("Default");

        // Assert
        Assert.Equal("SuccessData", value);
    }

    [Fact]
    public void ValueOrDefault_WhenFailure_ShouldReturnDefaultValue()
    {
        // Arrange
        var result = Result.Failure<string>(Error.NotFound());

        // Act
        var value = result.ValueOrDefault("Default");

        // Assert
        Assert.Equal("Default", value);
    }

    [Fact]
    public void Success_WithMetadata_ShouldStoreMetadata()
    {
        // Arrange
        var metadata = new Dictionary<string, object> { { "Version", "1.0" } };

        // Act
        var result = Result.Success(metadata);

        // Assert
        result.Should().BeSuccess();
        Assert.Equal("1.0", result.Metadata!["Version"]);
    }

    [Fact]
    public void HaveData_WithPredicate_WhenValueMatches_ShouldNotThrow()
    {
        // Arrange
        var result = Result.Success(10);

        // Act & Assert
        result.Should().HaveData(x => x > 5);
    }

    [Fact]
    public void HaveMetadata_WithValue_WhenKeyAndValueMatch_ShouldSucceed()
    {
        // Arrange
        var error = Error.Validation("V.1", "Message") with { Metadata = new Dictionary<string, object> { { "Key", "Value" } } };

        // Act & Assert
        error.Should().HaveMetadata("Key", "Value");
        
        // Asserting that it throws when metadata is wrong (to cover failure paths in assertions)
        Assert.ThrowsAny<Exception>(() => error.Should().HaveMetadata("Key", "WrongValue"));
        Assert.ThrowsAny<Exception>(() => error.Should().HaveMetadata("MissingKey", "Value"));
    }

    [Fact]
    public void Serialization_WithSuccessMetadata_ShouldSerializeCorrectly()
    {
        // Arrange
        var metadata = new Dictionary<string, object> { { "Warning", "Legacy API" } };
        var result = Result.Success("Data", metadata);
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        // Act
        var json = JsonSerializer.Serialize(result, options);
        var deserialized = JsonSerializer.Deserialize<Result<string>>(json, options);

        // Assert
        Assert.NotNull(deserialized);
        deserialized.Should().BeSuccess().HaveData("Data");
        Assert.Equal("Legacy API", deserialized.Metadata!["Warning"].ToString());
    }

    [Fact]
    public void Failure_WithMetadata_ShouldStoreMetadataInResult()
    {
        // Arrange
        var error = Error.NotFound();
        var metadata = new Dictionary<string, object> { { "Key", "Value" } };

        // Act
        var result = Result.Failure(error, metadata);

        // Assert
        result.Should().BeFailure().HaveMetadata("Key", "Value");
    }

    [Fact]
    public void Failure_WithCodeMessageAndMetadata_ShouldStoreEverything()
    {
        // Arrange
        const string code = "Code";
        const string message = "Message";
        var metadata = new Dictionary<string, object> { { "Key", "Value" } };

        // Act
        var result = Result.Failure(code, message, metadata);

        // Assert
        result.Should().BeFailure().HaveError(code).And.HaveMetadata("Key", "Value");
    }

    [Fact]
    public void FailureGeneric_WithCodeMessageAndMetadata_ShouldStoreEverything()
    {
        // Arrange
        const string code = "Code";
        const string message = "Message";
        var metadata = new Dictionary<string, object> { { "Key", "Value" } };

        // Act
        var result = Result.Failure<int>(code, message, metadata);

        // Assert
        result.Should().BeFailure().HaveError(code).And.HaveMetadata("Key", "Value");
    }

    [Fact]
    public void FailureGeneric_WithErrorAndMetadata_ShouldStoreEverything()
    {
        // Arrange
        var error = Error.NotFound();
        var metadata = new Dictionary<string, object> { { "Key", "Value" } };

        // Act
        var result = Result.Failure<int>(error, metadata);

        // Assert
        result.Should().BeFailure().HaveError(error.Code).And.HaveMetadata("Key", "Value");
    }

    [Fact]
    public async Task BindAsync_Result_WhenFailure_ShouldNotExecuteNext()
    {
        // Arrange
        var result = Result.Failure(Error.NotFound());
        var executed = false;

        // Act
        var bound = await result.BindAsync(() => { executed = true; return Task.FromResult(Result.Success()); });

        // Assert
        Assert.False(executed);
        bound.Should().BeFailure();
    }

    [Fact]
    public void Tap_WhenFailure_ShouldNotExecuteAction()
    {
        // Arrange
        var result = Result.Failure(Error.NotFound());
        var executed = false;

        // Act
        var tapped = result.Tap(() => executed = true);

        // Assert
        Assert.False(executed);
        tapped.Should().BeFailure();
    }

    [Fact]
    public async Task TapAsync_WhenFailure_ShouldNotExecuteFunc()
    {
        // Arrange
        var result = Result.Failure(Error.NotFound());
        var executed = false;

        // Act
        var tapped = await result.TapAsync(() => { executed = true; return Task.CompletedTask; });

        // Assert
        Assert.False(executed);
        tapped.Should().BeFailure();
    }

    [Fact]
    public void TapGeneric_WhenFailure_ShouldNotExecuteAction()
    {
        // Arrange
        var result = Result.Failure<int>(Error.NotFound());
        var executed = false;

        // Act
        var tapped = result.Tap(_ => executed = true);

        // Assert
        Assert.False(executed);
        tapped.Should().BeFailure();
    }

    [Fact]
    public async Task TapAsyncGeneric_WhenFailure_ShouldNotExecuteFunc()
    {
        // Arrange
        var result = Result.Failure<int>(Error.NotFound());
        var executed = false;

        // Act
        var tapped = await result.TapAsync(_ => { executed = true; return Task.CompletedTask; });

        // Assert
        Assert.False(executed);
        tapped.Should().BeFailure();
    }

    [Fact]
    public void Combine_WithZeroResults_ShouldReturnSuccess()
    {
        // Act
        var result = Result.Combine();

        // Assert
        result.Should().BeSuccess();
    }

    [Fact]
    public void Combine_WithOneSuccess_ShouldReturnSuccess()
    {
        // Act
        var result = Result.Combine(Result.Success());

        // Assert
        result.Should().BeSuccess();
    }

    [Fact]
    public void ValueOrThrow_WhenFailureWithNullFactory_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var result = Result.Failure<string>(Error.NotFound());

        // Act & Assert
        var exception = result.Should().ThrowOnValueAccess<InvalidOperationException>(null);
        Assert.Contains(ErrorCodes.NotFound, exception.Message);
    }

    [Fact]
    public void Result_ProtectedConstructor_ShouldBeAccessibleByInheritance()
    {
        // Act
        var result = new DerivedResult();

        // Assert
        Assert.True(result.IsSuccess);
    }

    private class DerivedResult : Result { }

    [Fact]
    public void CreateFailure_WhenInvalidType_ShouldThrowTypeInitializationException()
    {
        // Arrange
        var errors = new[] { Error.NotFound() };

        // Act & Assert
        var exception = Assert.Throws<TypeInitializationException>(() => Result.CreateFailure<DerivedResult>(errors));
        Assert.IsType<InvalidOperationException>(exception.InnerException);
    }

    [Fact]
    public void CreateFailure_ShouldUseOptimizedFactoryForResult()
    {
        // Arrange
        var errors = new[] { Error.NotFound() };

        // Act
        var result = Result.CreateFailure<Result>(errors);

        // Assert
        result.Should().BeFailure().HaveError(ErrorCodes.NotFound);
    }

    [Fact]
    public void CreateFailure_ShouldUseOptimizedFactoryForResultGeneric()
    {
        // Arrange
        var errors = new[] { Error.NotFound() };

        // Act
        var result = Result.CreateFailure<Result<string>>(errors);

        // Assert
        result.Should().BeFailure().HaveError(ErrorCodes.NotFound);
    }
}
