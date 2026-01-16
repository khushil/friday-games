namespace OXS.Tests.Core.AI;

public class AdaptiveAITests {
    [Fact]
    public void SelectMove_InitialDifficulty_UsesMedium() {
        // Arrange
        var ai = new AdaptiveAI();
        var board = new Board(3);

        // Act
        var difficulty = ai.CurrentDifficulty;

        // Assert
        difficulty.Should().Be(AIDifficulty.Medium);
    }

    [Fact]
    public void RecordGameResult_3PlayerWins_IncreasesDifficulty() {
        // Arrange
        var ai = new AdaptiveAI();

        // Act - player wins 3 times
        ai.RecordGameResult(isPlayerWin: true);
        ai.RecordGameResult(isPlayerWin: true);
        ai.RecordGameResult(isPlayerWin: true);

        // Assert
        ai.CurrentDifficulty.Should().Be(AIDifficulty.Hard);
    }

    [Fact]
    public void RecordGameResult_3AIWins_DecreasesDifficulty() {
        // Arrange
        var ai = new AdaptiveAI();

        // Act - AI wins 3 times
        ai.RecordGameResult(isPlayerWin: false);
        ai.RecordGameResult(isPlayerWin: false);
        ai.RecordGameResult(isPlayerWin: false);

        // Assert
        ai.CurrentDifficulty.Should().Be(AIDifficulty.Easy);
    }

    [Fact]
    public void RecordGameResult_AtMaxDifficulty_StaysAtMax() {
        // Arrange
        var ai = new AdaptiveAI();
        // Move to Hard
        ai.RecordGameResult(isPlayerWin: true);
        ai.RecordGameResult(isPlayerWin: true);
        ai.RecordGameResult(isPlayerWin: true);
        ai.CurrentDifficulty.Should().Be(AIDifficulty.Hard);

        // Act - player wins 3 more times
        ai.RecordGameResult(isPlayerWin: true);
        ai.RecordGameResult(isPlayerWin: true);
        ai.RecordGameResult(isPlayerWin: true);

        // Assert - should stay at Hard (max)
        ai.CurrentDifficulty.Should().Be(AIDifficulty.Hard);
    }
}
