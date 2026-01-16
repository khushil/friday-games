namespace OXS.Core.AI;

public sealed class EasyAI : IAIPlayer {
    private readonly Random _random = new();

    public Move SelectMove(Board board, PlayerId player, int winLength) {
        var validMoves = GameRules.GetValidMoves(board).ToList();
        if (validMoves.Count == 0) {
            throw new InvalidOperationException("No valid moves available");
        }
        return validMoves[_random.Next(validMoves.Count)];
    }
}
