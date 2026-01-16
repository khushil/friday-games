namespace OXS.Core.AI;

public sealed class AdaptiveAI : IAIPlayer {
    private const int WinsToAdjust = 3;

    private readonly EasyAI _easyAI = new();
    private readonly MediumAI _mediumAI = new();
    private readonly HardAI _hardAI = new();

    private int _consecutivePlayerWins;
    private int _consecutiveAIWins;

    public AIDifficulty CurrentDifficulty { get; private set; } = AIDifficulty.Medium;

    public Move SelectMove(Board board, PlayerId player, int winLength) {
        return CurrentDifficulty switch {
            AIDifficulty.Easy => _easyAI.SelectMove(board, player, winLength),
            AIDifficulty.Medium => _mediumAI.SelectMove(board, player, winLength),
            AIDifficulty.Hard => _hardAI.SelectMove(board, player, winLength),
            _ => _mediumAI.SelectMove(board, player, winLength)
        };
    }

    public void RecordGameResult(bool isPlayerWin) {
        if (isPlayerWin) {
            _consecutivePlayerWins++;
            _consecutiveAIWins = 0;

            if (_consecutivePlayerWins >= WinsToAdjust) {
                IncreaseDifficulty();
                _consecutivePlayerWins = 0;
            }
        } else {
            _consecutiveAIWins++;
            _consecutivePlayerWins = 0;

            if (_consecutiveAIWins >= WinsToAdjust) {
                DecreaseDifficulty();
                _consecutiveAIWins = 0;
            }
        }
    }

    private void IncreaseDifficulty() {
        CurrentDifficulty = CurrentDifficulty switch {
            AIDifficulty.Easy => AIDifficulty.Medium,
            AIDifficulty.Medium => AIDifficulty.Hard,
            AIDifficulty.Hard => AIDifficulty.Hard, // Stay at max
            _ => AIDifficulty.Medium
        };
    }

    private void DecreaseDifficulty() {
        CurrentDifficulty = CurrentDifficulty switch {
            AIDifficulty.Hard => AIDifficulty.Medium,
            AIDifficulty.Medium => AIDifficulty.Easy,
            AIDifficulty.Easy => AIDifficulty.Easy, // Stay at min
            _ => AIDifficulty.Medium
        };
    }
}
