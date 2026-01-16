namespace OXS.Tests.Core.AI;

public class HardAITests {
    [Fact]
    public void SelectMove_CanWinImmediately_TakesWinningMove() {
        // Arrange
        var ai = new HardAI();
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
        var ai = new HardAI();
        // X has no winning move, O can win at (1,2)
        var board = new Board(3)
            .WithMove(0, 0, PlayerId.X)
            .WithMove(1, 0, PlayerId.O)
            .WithMove(2, 2, PlayerId.X)
            .WithMove(1, 1, PlayerId.O);

        // Act
        var move = ai.SelectMove(board, PlayerId.X, 3);

        // Assert
        move.Should().Be(new Move(1, 2));
    }

    [Fact]
    public void SelectMove_EmptyBoard_ReturnsValidMove() {
        // Arrange
        var ai = new HardAI();
        var board = new Board(3);

        // Act
        var move = ai.SelectMove(board, PlayerId.X, 3);

        // Assert
        move.Row.Should().BeInRange(0, 2);
        move.Col.Should().BeInRange(0, 2);
    }

    [Fact]
    public void SelectMove_OptimalPlay_NeverLoses() {
        // Arrange
        var hardAI = new HardAI();
        var easyAI = new EasyAI();
        var losses = 0;

        // Act - play 50 games as X and 50 as O
        for (int i = 0; i < 50; i++) {
            // HardAI as X
            var resultAsX = PlayGame(hardAI, easyAI);
            if (resultAsX is GameResult.Win win1 && win1.Winner == PlayerId.O) {
                losses++;
            }

            // HardAI as O
            var resultAsO = PlayGame(easyAI, hardAI);
            if (resultAsO is GameResult.Win win2 && win2.Winner == PlayerId.X) {
                losses++;
            }
        }

        // Assert
        losses.Should().Be(0);
    }

    [Fact]
    public void SelectMove_FromForkPosition_FindsBestMove() {
        // Arrange - Classic fork position where X can create a fork
        // X . .
        // . O .
        // . . X
        var ai = new HardAI();
        var board = new Board(3)
            .WithMove(0, 0, PlayerId.X)
            .WithMove(1, 1, PlayerId.O)
            .WithMove(2, 2, PlayerId.X);

        // Act - O must block the fork. Best moves are edges (0,1), (1,0), (1,2), (2,1)
        var move = ai.SelectMove(board, PlayerId.O, 3);

        // Assert - should pick an edge to prevent the fork
        var edges = new[] { new Move(0, 1), new Move(1, 0), new Move(1, 2), new Move(2, 1) };
        edges.Should().Contain(move);
    }

    [Fact]
    public void SelectMove_CompletesWithinTimeout() {
        // Arrange
        var ai = new HardAI();
        var board = new Board(3);

        // Act & Assert - should complete within 5 seconds
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        ai.SelectMove(board, PlayerId.X, 3);
        stopwatch.Stop();

        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000);
    }

    private static GameResult PlayGame(IAIPlayer xPlayer, IAIPlayer oPlayer) {
        var board = new Board(3);
        var currentPlayer = PlayerId.X;

        while (true) {
            var ai = currentPlayer == PlayerId.X ? xPlayer : oPlayer;
            var move = ai.SelectMove(board, currentPlayer, 3);
            board = board.WithMove(move.Row, move.Col, currentPlayer);

            var result = GameRules.CheckResult(board, 3);
            if (result is not GameResult.InProgress) {
                return result;
            }

            currentPlayer = currentPlayer.GetOpponent();
        }
    }
}
