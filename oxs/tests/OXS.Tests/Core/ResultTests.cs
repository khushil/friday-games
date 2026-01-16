namespace OXS.Tests.Core;

public class ResultTests {
    [Fact]
    public void Success_IsSuccess_ReturnsTrue() {
        // Arrange & Act
        var result = new Result<int>.Success(42);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
    }

    [Fact]
    public void Failure_IsFailure_ReturnsTrue() {
        // Arrange & Act
        var result = new Result<int>.Failure("Error message");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Match_OnSuccess_CallsOnSuccess() {
        // Arrange
        var result = new Result<int>.Success(42);
        var called = false;

        // Act
        var output = result.Match(
            onSuccess: v => { called = true; return v * 2; },
            onFailure: _ => -1
        );

        // Assert
        called.Should().BeTrue();
        output.Should().Be(84);
    }

    [Fact]
    public void Match_OnFailure_CallsOnFailure() {
        // Arrange
        var result = new Result<int>.Failure("Error");
        var errorMessage = "";

        // Act
        var output = result.Match(
            onSuccess: v => v,
            onFailure: e => { errorMessage = e; return -1; }
        );

        // Assert
        errorMessage.Should().Be("Error");
        output.Should().Be(-1);
    }
}
