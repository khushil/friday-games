namespace OXS.Core.AI;

public sealed class HardAI : IAIPlayer {
    public Move SelectMove(Board board, PlayerId player, int winLength) {
        int emptyCount = board.Cells.Count(c => c == CellState.Empty);
        
        // Dynamic depth: deeper for smaller boards/fewer moves
        int maxDepth = board.Size switch {
            3 => 9,
            4 => 6,
            _ => emptyCount > 15 ? 3 : emptyCount > 10 ? 4 : 5
        };
        
        var (_, bestMove) = Minimax(board, player, player, winLength, maxDepth, int.MinValue, int.MaxValue);
        return bestMove ?? GameRules.GetValidMoves(board).First();
    }

    private static (int Score, Move? Move) Minimax(
        Board board,
        PlayerId currentPlayer,
        PlayerId maximisingPlayer,
        int winLength,
        int depth,
        int alpha,
        int beta) {
        var result = GameRules.CheckResult(board, winLength);

        if (result is GameResult.Win win) {
            int score = win.Winner == maximisingPlayer ? 100 + depth : -100 - depth;
            return (score, null);
        }

        if (result is GameResult.Draw || depth == 0) {
            return (EvaluateBoard(board, maximisingPlayer, winLength), null);
        }

        var validMoves = GetOrderedMoves(board);
        Move? bestMove = null;

        if (currentPlayer == maximisingPlayer) {
            int maxEval = int.MinValue;
            foreach (var move in validMoves) {
                var newBoard = board.WithMove(move.Row, move.Col, currentPlayer);
                var (eval, _) = Minimax(newBoard, currentPlayer.GetOpponent(), maximisingPlayer, winLength, depth - 1, alpha, beta);

                if (eval > maxEval) {
                    maxEval = eval;
                    bestMove = move;
                }
                alpha = Math.Max(alpha, eval);
                if (beta <= alpha) {
                    break;
                }
            }
            return (maxEval, bestMove);
        } else {
            int minEval = int.MaxValue;
            foreach (var move in validMoves) {
                var newBoard = board.WithMove(move.Row, move.Col, currentPlayer);
                var (eval, _) = Minimax(newBoard, currentPlayer.GetOpponent(), maximisingPlayer, winLength, depth - 1, alpha, beta);

                if (eval < minEval) {
                    minEval = eval;
                    bestMove = move;
                }
                beta = Math.Min(beta, eval);
                if (beta <= alpha) {
                    break;
                }
            }
            return (minEval, bestMove);
        }
    }

    private static List<Move> GetOrderedMoves(Board board) {
        var moves = GameRules.GetValidMoves(board).ToList();
        int center = board.Size / 2;
        
        // Order: center first, then by distance from center (closer is better)
        return moves
            .OrderBy(m => Math.Abs(m.Row - center) + Math.Abs(m.Col - center))
            .ToList();
    }

    private static int EvaluateBoard(Board board, PlayerId player, int winLength) {
        // Simple heuristic: count potential winning lines
        int score = 0;
        var opponent = player.GetOpponent();
        
        // Check rows, columns, and diagonals for partial lines
        for (int row = 0; row < board.Size; row++) {
            for (int col = 0; col < board.Size; col++) {
                score += EvaluatePosition(board, row, col, player, opponent);
            }
        }
        
        return score;
    }

    private static int EvaluatePosition(Board board, int row, int col, PlayerId player, PlayerId opponent) {
        var state = board[row, col];
        if (state == CellState.Empty) return 0;

        int multiplier = state == (player == PlayerId.X ? CellState.X : CellState.O) ? 1 : -1;
        int center = board.Size / 2;
        int distFromCenter = Math.Abs(row - center) + Math.Abs(col - center);

        // Prefer center positions
        return multiplier * (board.Size - distFromCenter);
    }
}
