namespace OXS.Tests.Core;

public class GameRulesTests {
    [Fact]
    public void CheckResult_EmptyBoard_ReturnsInProgress() {
        // Arrange
        var board = new Board(3);

        // Act
        var result = GameRules.CheckResult(board, 3);

        // Assert
        result.Should().BeOfType<GameResult.InProgress>();
    }

    [Theory]
    [InlineData(0, 1, 2)] // Top row
    [InlineData(3, 4, 5)] // Middle row
    [InlineData(6, 7, 8)] // Bottom row
    public void CheckResult_ThreeXsInRow_ReturnsXWins(int a, int b, int c) {
        // Arrange
        var board = new Board(3)
            .WithMove(Move.FromIndex(a, 3).Row, Move.FromIndex(a, 3).Col, PlayerId.X)
            .WithMove(Move.FromIndex(b, 3).Row, Move.FromIndex(b, 3).Col, PlayerId.X)
            .WithMove(Move.FromIndex(c, 3).Row, Move.FromIndex(c, 3).Col, PlayerId.X);

        // Act
        var result = GameRules.CheckResult(board, 3);

        // Assert
        result.Should().BeOfType<GameResult.Win>()
            .Which.Winner.Should().Be(PlayerId.X);
    }

    [Theory]
    [InlineData(0, 3, 6)] // Left column
    [InlineData(1, 4, 7)] // Middle column
    [InlineData(2, 5, 8)] // Right column
    public void CheckResult_ThreeOsInColumn_ReturnsOWins(int a, int b, int c) {
        // Arrange
        var board = new Board(3)
            .WithMove(Move.FromIndex(a, 3).Row, Move.FromIndex(a, 3).Col, PlayerId.O)
            .WithMove(Move.FromIndex(b, 3).Row, Move.FromIndex(b, 3).Col, PlayerId.O)
            .WithMove(Move.FromIndex(c, 3).Row, Move.FromIndex(c, 3).Col, PlayerId.O);

        // Act
        var result = GameRules.CheckResult(board, 3);

        // Assert
        result.Should().BeOfType<GameResult.Win>()
            .Which.Winner.Should().Be(PlayerId.O);
    }

    [Theory]
    [InlineData(0, 4, 8)] // Main diagonal
    [InlineData(2, 4, 6)] // Anti-diagonal
    public void CheckResult_ThreeInDiagonal_ReturnsWin(int a, int b, int c) {
        // Arrange
        var board = new Board(3)
            .WithMove(Move.FromIndex(a, 3).Row, Move.FromIndex(a, 3).Col, PlayerId.X)
            .WithMove(Move.FromIndex(b, 3).Row, Move.FromIndex(b, 3).Col, PlayerId.X)
            .WithMove(Move.FromIndex(c, 3).Row, Move.FromIndex(c, 3).Col, PlayerId.X);

        // Act
        var result = GameRules.CheckResult(board, 3);

        // Assert
        result.Should().BeOfType<GameResult.Win>();
    }

    [Fact]
    public void CheckResult_FullBoardNoWinner_ReturnsDraw() {
        // Arrange - Classic draw position
        // X O X
        // X X O
        // O X O
        var board = new Board(3)
            .WithMove(0, 0, PlayerId.X).WithMove(0, 1, PlayerId.O).WithMove(0, 2, PlayerId.X)
            .WithMove(1, 0, PlayerId.X).WithMove(1, 1, PlayerId.X).WithMove(1, 2, PlayerId.O)
            .WithMove(2, 0, PlayerId.O).WithMove(2, 1, PlayerId.X).WithMove(2, 2, PlayerId.O);

        // Act
        var result = GameRules.CheckResult(board, 3);

        // Assert
        result.Should().BeOfType<GameResult.Draw>();
    }

    [Fact]
    public void FindWinningLine_HasWin_ReturnsCorrectCells() {
        // Arrange - Top row win
        var board = new Board(3)
            .WithMove(0, 0, PlayerId.X)
            .WithMove(0, 1, PlayerId.X)
            .WithMove(0, 2, PlayerId.X);

        // Act
        var winLine = GameRules.FindWinningLine(board, 3);

        // Assert
        winLine.Should().NotBeNull();
        winLine!.Value.Should().HaveCount(3);
    }

    [Fact]
    public void GetAllLines_3x3Board_Returns8Lines() {
        // 3 rows + 3 columns + 2 diagonals = 8 lines
        // Act
        var lines = GameRules.GetAllLines(3, 3).ToList();

        // Assert
        lines.Should().HaveCount(8);
    }

    [Fact]
    public void GetAllLines_4x4BoardWinLength3_ReturnsCorrectCount() {
        // 4x4 with win length 3:
        // Horizontal: 4 rows * 2 positions = 8
        // Vertical: 4 cols * 2 positions = 8
        // Diagonals: (2*2)*2 = 8
        // Total = 24
        // Act
        var lines = GameRules.GetAllLines(4, 3).ToList();

        // Assert
        lines.Count.Should().Be(24);
    }

    [Fact]
    public void GetValidMoves_EmptyBoard_Returns9Moves() {
        // Arrange
        var board = new Board(3);

        // Act
        var moves = GameRules.GetValidMoves(board).ToList();

        // Assert
        moves.Should().HaveCount(9);
    }

    [Fact]
    public void GetValidMoves_PartialBoard_ReturnsOnlyEmpty() {
        // Arrange
        var board = new Board(3)
            .WithMove(0, 0, PlayerId.X)
            .WithMove(1, 1, PlayerId.O);

        // Act
        var moves = GameRules.GetValidMoves(board).ToList();

        // Assert
        moves.Should().HaveCount(7);
        moves.Should().NotContain(new Move(0, 0));
        moves.Should().NotContain(new Move(1, 1));
    }
}
