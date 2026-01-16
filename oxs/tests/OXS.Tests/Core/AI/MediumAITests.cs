namespace OXS.Tests.Core.AI;

public class MediumAITests {
    [Fact]
    public void SelectMove_CanWin_TakesWinningMove() {
        // Arrange
        var ai = new MediumAI();
        // X has two in a row, can win at (0,2)
        var board = new Board(3)
            .WithMove(0, 0, PlayerId.X)
            .WithMove(0, 1, PlayerId.X)
            .WithMove(1, 0, PlayerId.O)
            .WithMove(1, 1, PlayerId.O);

        // Act
        var move = ai.SelectMove(board, PlayerId.X, 3);

        // Assert
        move.Should().Be(new Move(0, 2));
    }

    [Fact]
    public void SelectMove_OpponentCanWin_BlocksThem() {
        // Arrange
        var ai = new MediumAI();
        // Board setup where X has no winning move, but O can win at (1,2)
        // X . .
        // O O .
        // . . X
        var board = new Board(3)
            .WithMove(0, 0, PlayerId.X)
            .WithMove(1, 0, PlayerId.O)
            .WithMove(2, 2, PlayerId.X)
            .WithMove(1, 1, PlayerId.O);

        // Act - X's turn, should block O's winning move at (1,2)
        var move = ai.SelectMove(board, PlayerId.X, 3);

        // Assert
        move.Should().Be(new Move(1, 2));
    }

    [Fact]
    public void SelectMove_CenterOpen_TakesCenter() {
        // Arrange
        var ai = new MediumAI();
        // X plays corner, O should take center
        var board = new Board(3)
            .WithMove(0, 0, PlayerId.X);

        // Act
        var move = ai.SelectMove(board, PlayerId.O, 3);

        // Assert
        move.Should().Be(new Move(1, 1));
    }

    [Fact]
    public void SelectMove_NoWinOrBlock_ReturnsValidMove() {
        // Arrange
        var ai = new MediumAI();
        // A position where there's no immediate win or block needed
        var board = new Board(3)
            .WithMove(1, 1, PlayerId.X); // X takes center

        // Act
        var move = ai.SelectMove(board, PlayerId.O, 3);

        // Assert
        board.IsCellEmpty(move.Row, move.Col).Should().BeTrue();
    }

    [Fact]
    public void SelectMove_EmptyBoard_ReturnsValidMove() {
        // Arrange
        var ai = new MediumAI();
        var board = new Board(3);

        // Act
        var move = ai.SelectMove(board, PlayerId.X, 3);

        // Assert
        move.Row.Should().BeInRange(0, 2);
        move.Col.Should().BeInRange(0, 2);
    }
}
