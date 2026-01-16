namespace OXS.Tests.Core;

public class GameStateMachineTests {
    [Fact]
    public void Constructor_CreatesEmptyBoard() {
        // Arrange & Act
        var game = new GameStateMachine(boardSize: 3, winLength: 3);

        // Assert
        game.Board.Cells.Should().AllBeEquivalentTo(CellState.Empty);
    }

    [Fact]
    public void Constructor_XStartsFirst() {
        // Arrange & Act
        var game = new GameStateMachine(boardSize: 3, winLength: 3);

        // Assert
        game.CurrentPlayer.Should().Be(PlayerId.X);
    }

    [Fact]
    public void MakeMove_ValidMove_UpdatesBoard() {
        // Arrange
        var game = new GameStateMachine(boardSize: 3, winLength: 3);

        // Act
        var result = game.MakeMove(0, 0);

        // Assert
        result.IsSuccess.Should().BeTrue();
        game.Board[0, 0].Should().Be(CellState.X);
    }

    [Fact]
    public void MakeMove_ValidMove_SwitchesPlayer() {
        // Arrange
        var game = new GameStateMachine(boardSize: 3, winLength: 3);

        // Act
        game.MakeMove(0, 0);

        // Assert
        game.CurrentPlayer.Should().Be(PlayerId.O);
    }

    [Fact]
    public void MakeMove_WrongPlayer_ReturnsFailure() {
        // Arrange
        var game = new GameStateMachine(boardSize: 3, winLength: 3);
        game.MakeMove(0, 0); // X moves

        // Act - Try to move as X again (should be O's turn)
        var result = game.MakeMove(1, 1, PlayerId.X);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void MakeMove_OccupiedCell_ReturnsFailure() {
        // Arrange
        var game = new GameStateMachine(boardSize: 3, winLength: 3);
        game.MakeMove(0, 0);

        // Act
        var result = game.MakeMove(0, 0);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void MakeMove_WinningMove_SetsPhaseGameOver() {
        // Arrange
        var game = new GameStateMachine(boardSize: 3, winLength: 3);
        // X: (0,0), O: (1,0), X: (0,1), O: (1,1), X: (0,2) - X wins top row
        game.MakeMove(0, 0); // X
        game.MakeMove(1, 0); // O
        game.MakeMove(0, 1); // X
        game.MakeMove(1, 1); // O

        // Act
        game.MakeMove(0, 2); // X wins

        // Assert
        game.Phase.Should().Be(GamePhase.GameOver);
    }

    [Fact]
    public void MakeMove_WinningMove_RaisesGameEnded() {
        // Arrange
        var game = new GameStateMachine(boardSize: 3, winLength: 3);
        GameResult? capturedResult = null;
        game.GameEnded += result => capturedResult = result;

        // X: (0,0), O: (1,0), X: (0,1), O: (1,1), X: (0,2) - X wins top row
        game.MakeMove(0, 0); // X
        game.MakeMove(1, 0); // O
        game.MakeMove(0, 1); // X
        game.MakeMove(1, 1); // O

        // Act
        game.MakeMove(0, 2); // X wins

        // Assert
        capturedResult.Should().NotBeNull();
        capturedResult.Should().BeOfType<GameResult.Win>()
            .Which.Winner.Should().Be(PlayerId.X);
    }

    [Fact]
    public void StartNextRound_ResetsBoard() {
        // Arrange
        var game = new GameStateMachine(boardSize: 3, winLength: 3);
        game.MakeMove(0, 0);
        game.MakeMove(1, 0);
        game.MakeMove(0, 1);
        game.MakeMove(1, 1);
        game.MakeMove(0, 2); // X wins

        // Act
        game.StartNextRound(PlayerId.O);

        // Assert
        game.Board.Cells.Should().AllBeEquivalentTo(CellState.Empty);
        game.Phase.Should().Be(GamePhase.Playing);
    }

    [Fact]
    public void StartNextRound_SetsSpecifiedStartingPlayer() {
        // Arrange
        var game = new GameStateMachine(boardSize: 3, winLength: 3);
        game.MakeMove(0, 0);
        game.MakeMove(1, 0);
        game.MakeMove(0, 1);
        game.MakeMove(1, 1);
        game.MakeMove(0, 2); // X wins

        // Act
        game.StartNextRound(PlayerId.O);

        // Assert
        game.CurrentPlayer.Should().Be(PlayerId.O);
    }
}
