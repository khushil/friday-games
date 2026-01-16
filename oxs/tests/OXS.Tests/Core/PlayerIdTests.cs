namespace OXS.Tests.Core;

public class PlayerIdTests {
    [Fact]
    public void X_HasValue0() {
        // Arrange & Act
        var x = PlayerId.X;

        // Assert
        x.Value.Should().Be(0);
    }

    [Fact]
    public void O_HasValue1() {
        // Arrange & Act
        var o = PlayerId.O;

        // Assert
        o.Value.Should().Be(1);
    }

    [Fact]
    public void GetOpponent_OfX_ReturnsO() {
        // Arrange
        var x = PlayerId.X;

        // Act
        var opponent = x.GetOpponent();

        // Assert
        opponent.Should().Be(PlayerId.O);
    }

    [Fact]
    public void GetOpponent_OfO_ReturnsX() {
        // Arrange
        var o = PlayerId.O;

        // Act
        var opponent = o.GetOpponent();

        // Assert
        opponent.Should().Be(PlayerId.X);
    }
}
