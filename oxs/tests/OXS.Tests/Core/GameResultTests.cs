namespace OXS.Tests.Core;

public class GameResultTests {
    [Fact]
    public void Win_ContainsWinner() {
        // Arrange & Act
        var winLine = ImmutableArray.Create(new Move(0, 0), new Move(0, 1), new Move(0, 2));
        var result = new GameResult.Win(PlayerId.X, winLine);

        // Assert
        result.Winner.Should().Be(PlayerId.X);
    }

    [Fact]
    public void Win_ContainsWinningLine() {
        // Arrange
        var winLine = ImmutableArray.Create(new Move(0, 0), new Move(0, 1), new Move(0, 2));

        // Act
        var result = new GameResult.Win(PlayerId.O, winLine);

        // Assert
        result.WinningLine.Should().HaveCount(3);
        result.WinningLine[0].Should().Be(new Move(0, 0));
    }

    [Fact]
    public void Draw_IsDistinctType() {
        // Arrange & Act
        GameResult result = new GameResult.Draw();

        // Assert
        result.Should().BeOfType<GameResult.Draw>();
    }

    [Fact]
    public void InProgress_IsDistinctType() {
        // Arrange & Act
        GameResult result = new GameResult.InProgress();

        // Assert
        result.Should().BeOfType<GameResult.InProgress>();
    }
}
