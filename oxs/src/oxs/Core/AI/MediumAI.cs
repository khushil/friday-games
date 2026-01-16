namespace OXS.Core.AI;

public sealed class MediumAI : IAIPlayer {
    private readonly Random _random = new();

    public Move SelectMove(Board board, PlayerId player, int winLength) {
        // 1. Try to win
        var winningMove = FindWinningMove(board, player, winLength);
        if (winningMove.HasValue) {
            return winningMove.Value;
        }

        // 2. Block opponent's winning move
        var blockMove = FindWinningMove(board, player.GetOpponent(), winLength);
        if (blockMove.HasValue) {
            return blockMove.Value;
        }

        // 3. Take center if available
        int center = board.Size / 2;
        if (board.IsCellEmpty(center, center)) {
            return new Move(center, center);
        }

        // 4. Random move
        var validMoves = GameRules.GetValidMoves(board).ToList();
        return validMoves[_random.Next(validMoves.Count)];
    }

    private static Move? FindWinningMove(Board board, PlayerId player, int winLength) {
        foreach (var move in GameRules.GetValidMoves(board)) {
            var newBoard = board.WithMove(move.Row, move.Col, player);
            var result = GameRules.CheckResult(newBoard, winLength);
            if (result is GameResult.Win win && win.Winner == player) {
                return move;
            }
        }
        return null;
    }
}
