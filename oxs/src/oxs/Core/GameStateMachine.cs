namespace OXS.Core;

public sealed class GameStateMachine {
    public Board Board { get; private set; }
    public PlayerId CurrentPlayer { get; private set; }
    public GamePhase Phase { get; private set; }
    public int WinLength { get; }

    public event Action<GameResult>? GameEnded;

    public GameStateMachine(int boardSize, int winLength) {
        Board = new Board(boardSize);
        CurrentPlayer = PlayerId.X;
        Phase = GamePhase.Playing;
        WinLength = winLength;
    }

    public Result<Board> MakeMove(int row, int col) {
        return MakeMove(row, col, CurrentPlayer);
    }

    public Result<Board> MakeMove(int row, int col, PlayerId player) {
        if (Phase == GamePhase.GameOver) {
            return new Result<Board>.Failure("Game is already over");
        }

        if (player != CurrentPlayer) {
            return new Result<Board>.Failure($"Not {player}'s turn");
        }

        if (!Board.IsCellEmpty(row, col)) {
            return new Result<Board>.Failure("Cell is already occupied");
        }

        Board = Board.WithMove(row, col, player);
        var result = GameRules.CheckResult(Board, WinLength);

        switch (result) {
            case GameResult.Win:
            case GameResult.Draw:
                Phase = GamePhase.GameOver;
                GameEnded?.Invoke(result);
                break;
            default:
                CurrentPlayer = CurrentPlayer.GetOpponent();
                break;
        }

        return new Result<Board>.Success(Board);
    }

    public void StartNextRound(PlayerId startingPlayer) {
        Board = new Board(Board.Size);
        CurrentPlayer = startingPlayer;
        Phase = GamePhase.Playing;
    }
}
