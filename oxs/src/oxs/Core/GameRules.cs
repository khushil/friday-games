namespace OXS.Core;

public static class GameRules {
    public static GameResult CheckResult(Board board, int winLength) {
        var winLine = FindWinningLine(board, winLength);
        return winLine switch {
            not null => new GameResult.Win(
                board[winLine.Value[0].Row, winLine.Value[0].Col] == CellState.X ? PlayerId.X : PlayerId.O,
                winLine.Value),
            null when board.IsFull => new GameResult.Draw(),
            _ => new GameResult.InProgress()
        };
    }

    public static ImmutableArray<Move>? FindWinningLine(Board board, int winLength) {
        var lines = GetAllLines(board.Size, winLength);
        foreach (var line in lines) {
            var first = board[line[0].Row, line[0].Col];
            if (first == CellState.Empty) {
                continue;
            }
            if (line.All(m => board[m.Row, m.Col] == first)) {
                return line;
            }
        }
        return null;
    }

    public static IEnumerable<ImmutableArray<Move>> GetAllLines(int boardSize, int winLength) {
        // Horizontal lines
        for (int row = 0; row < boardSize; row++) {
            for (int startCol = 0; startCol <= boardSize - winLength; startCol++) {
                yield return Enumerable.Range(0, winLength)
                    .Select(i => new Move(row, startCol + i))
                    .ToImmutableArray();
            }
        }

        // Vertical lines
        for (int col = 0; col < boardSize; col++) {
            for (int startRow = 0; startRow <= boardSize - winLength; startRow++) {
                yield return Enumerable.Range(0, winLength)
                    .Select(i => new Move(startRow + i, col))
                    .ToImmutableArray();
            }
        }

        // Diagonal lines (top-left to bottom-right)
        for (int row = 0; row <= boardSize - winLength; row++) {
            for (int col = 0; col <= boardSize - winLength; col++) {
                yield return Enumerable.Range(0, winLength)
                    .Select(i => new Move(row + i, col + i))
                    .ToImmutableArray();
            }
        }

        // Diagonal lines (top-right to bottom-left)
        for (int row = 0; row <= boardSize - winLength; row++) {
            for (int col = winLength - 1; col < boardSize; col++) {
                yield return Enumerable.Range(0, winLength)
                    .Select(i => new Move(row + i, col - i))
                    .ToImmutableArray();
            }
        }
    }

    public static IEnumerable<Move> GetValidMoves(Board board) {
        for (int row = 0; row < board.Size; row++) {
            for (int col = 0; col < board.Size; col++) {
                if (board.IsCellEmpty(row, col)) {
                    yield return new Move(row, col);
                }
            }
        }
    }
}
