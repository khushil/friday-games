namespace OXS.Tests.Core.AI;

public class EasyAITests {
    [Fact]
    public void SelectMove_EmptyBoard_ReturnsValidMove() {
        // Arrange
        var ai = new EasyAI();
        var board = new Board(3);

        // Act
        var move = ai.SelectMove(board, PlayerId.X, 3);

        // Assert
        move.Row.Should().BeInRange(0, 2);
        move.Col.Should().BeInRange(0, 2);
    }

    [Fact]
    public void SelectMove_OneEmptyCell_ReturnsThatCell() {
        // Arrange
        var ai = new EasyAI();
        // Fill all but one cell (bottom-right)
        var board = new Board(3)
            .WithMove(0, 0, PlayerId.X).WithMove(0, 1, PlayerId.O).WithMove(0, 2, PlayerId.X)
            .WithMove(1, 0, PlayerId.O).WithMove(1, 1, PlayerId.X).WithMove(1, 2, PlayerId.O)
            .WithMove(2, 0, PlayerId.X).WithMove(2, 1, PlayerId.O);
        // Only (2, 2) is empty

        // Act
        var move = ai.SelectMove(board, PlayerId.X, 3);

        // Assert
        move.Should().Be(new Move(2, 2));
    }

    [Fact]
    public void SelectMove_Called100Times_ReturnsVariety() {
        // Arrange
        var ai = new EasyAI();
        var board = new Board(3);
        var moves = new HashSet<Move>();

        // Act - call 100 times
        for (int i = 0; i < 100; i++) {
            moves.Add(ai.SelectMove(board, PlayerId.X, 3));
        }

        // Assert - should have more than 1 unique move (randomness)
        moves.Count.Should().BeGreaterThan(1);
    }
}
