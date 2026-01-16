namespace OXS.Tests.Core;

public class MoveTests {
    [Fact]
    public void ToIndex_Row1Col2_Returns5() {
        // Arrange
        var move = new Move(1, 2);

        // Act
        var index = move.ToIndex(3);

        // Assert
        index.Should().Be(5);
    }

    [Fact]
    public void FromIndex_Index5_ReturnsRow1Col2() {
        // Arrange & Act
        var move = Move.FromIndex(5, 3);

        // Assert
        move.Row.Should().Be(1);
        move.Col.Should().Be(2);
    }

    [Fact]
    public void Equality_SameRowCol_AreEqual() {
        // Arrange
        var move1 = new Move(1, 2);
        var move2 = new Move(1, 2);

        // Assert
        move1.Should().Be(move2);
    }
}
