namespace OXS.Core.AI;

public static class AIFactory {
    public static IAIPlayer Create(AIDifficulty difficulty) {
        return difficulty switch {
            AIDifficulty.Easy => new EasyAI(),
            AIDifficulty.Medium => new MediumAI(),
            AIDifficulty.Hard => new HardAI(),
            _ => throw new ArgumentOutOfRangeException(nameof(difficulty))
        };
    }
}
