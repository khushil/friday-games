namespace OXS.Tests.Core;

public class BoardTests {
    [Fact]
    public void Constructor_Size3_Creates9Cells() {
        // Arrange & Act
        var board = new Board(3);

        // Assert
        board.Size.Should().Be(3);
        board.Cells.Length.Should().Be(9);
    }

    [Fact]
    public void AllCells_NewBoard_AllEmpty() {
        // Arrange & Act
        var board = new Board(3);

        // Assert
        board.Cells.Should().AllBeEquivalentTo(CellState.Empty);
    }

    [Fact]
    public void WithMove_ValidMove_ReturnsNewBoard() {
        // Arrange
        var board = new Board(3);

        // Act
        var newBoard = board.WithMove(0, 0, PlayerId.X);

        // Assert
        newBoard[0, 0].Should().Be(CellState.X);
    }

    [Fact]
    public void WithMove_ValidMove_OriginalUnchanged() {
        // Arrange
        var board = new Board(3);

        // Act
        var newBoard = board.WithMove(1, 1, PlayerId.X);

        // Assert
        board[1, 1].Should().Be(CellState.Empty);
        newBoard[1, 1].Should().Be(CellState.X);
    }

    [Fact]
    public void WithMove_OccupiedCell_Throws() {
        // Arrange
        var board = new Board(3).WithMove(0, 0, PlayerId.X);

        // Act
        var act = () => board.WithMove(0, 0, PlayerId.O);

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Indexer_ValidPosition_ReturnsState() {
        // Arrange
        var board = new Board(3).WithMove(1, 2, PlayerId.O);

        // Act
        var state = board[1, 2];

        // Assert
        state.Should().Be(CellState.O);
    }

    [Fact]
    public void IsCellEmpty_EmptyCell_ReturnsTrue() {
        // Arrange
        var board = new Board(3);

        // Act & Assert
        board.IsCellEmpty(0, 0).Should().BeTrue();
    }

    [Fact]
    public void IsFull_AllOccupied_ReturnsTrue() {
        // Arrange - Fill all cells
        var board = new Board(3)
            .WithMove(0, 0, PlayerId.X).WithMove(0, 1, PlayerId.O).WithMove(0, 2, PlayerId.X)
            .WithMove(1, 0, PlayerId.O).WithMove(1, 1, PlayerId.X).WithMove(1, 2, PlayerId.O)
            .WithMove(2, 0, PlayerId.X).WithMove(2, 1, PlayerId.O).WithMove(2, 2, PlayerId.X);

        // Act & Assert
        board.IsFull.Should().BeTrue();
    }
}
