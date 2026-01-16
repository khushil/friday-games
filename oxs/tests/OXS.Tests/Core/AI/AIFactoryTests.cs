namespace OXS.Tests.Core.AI;

public class AIFactoryTests {
    [Fact]
    public void Create_Easy_ReturnsEasyAI() {
        // Act
        var ai = AIFactory.Create(AIDifficulty.Easy);

        // Assert
        ai.Should().BeOfType<EasyAI>();
    }

    [Fact]
    public void Create_Medium_ReturnsMediumAI() {
        // Act
        var ai = AIFactory.Create(AIDifficulty.Medium);

        // Assert
        ai.Should().BeOfType<MediumAI>();
    }

    [Fact]
    public void Create_Hard_ReturnsHardAI() {
        // Act
        var ai = AIFactory.Create(AIDifficulty.Hard);

        // Assert
        ai.Should().BeOfType<HardAI>();
    }
}
