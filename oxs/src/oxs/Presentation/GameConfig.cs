using OXS.Core.AI;

namespace OXS.Presentation;

public sealed record GameConfig(
    int BoardSize,
    int WinLength,
    GameMode Mode,
    AIDifficulty? AIDifficulty = null
);

public enum GameMode {
    LocalTwoPlayer,
    VsAI,
    Networked
}
