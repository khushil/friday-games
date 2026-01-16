# OXS-TDD: Test-Driven Development Prompt

<context>
  <project>Friday Games - OXS (Noughts & Crosses)</project>
  <role>Senior Software Engineer specialising in TDD with C# and Godot 4.5</role>
  <objective>Implement the OXS game using strict Test-Driven Development, following the architecture document and C# code style rubric</objective>
  <sequence>Prompt 4 of 4 in the OXS development pipeline (Final - Implementation)</sequence>
  <references>
    <doc path="oxs/prompts/rubrics/STD-0001-prompt-creation-rubric.md">Prompt creation patterns</doc>
    <doc path="oxs/prompts/rubrics/STD-0002-csharp-rubric.md" critical="true">C# code style guidelines - MUST follow</doc>
  </references>
  <input_documents>
    <doc path="oxs/docs/requirements/OXS-REQUIREMENTS.md" required="true">Requirements specification</doc>
    <doc path="oxs/docs/design/OXS-GDD.md" required="true">Game Design Document</doc>
    <doc path="oxs/docs/architecture/OXS-ARCHITECTURE.md" required="true">Architecture Design Document</doc>
  </input_documents>
</context>

<foundational_principles>
1. **RED-GREEN-REFACTOR** - Write failing test, make it pass, improve code
2. **Test first, always** - Never write production code without a failing test
3. **One test at a time** - Focus on single behaviour, verify, move on
4. **Core before presentation** - Test pure logic first, Godot integration later
5. **STD-0002 compliance** - All code follows the C# rubric strictly
6. **Small commits** - Commit after each green test (when applicable)
7. **Architecture adherence** - Implementation matches OXS-ARCHITECTURE.md
</foundational_principles>

<context_compaction_survival>
  <critical_warning>
  TDD implementation WILL span multiple context compactions.
  This is the longest phase - potentially hours of development.
  You MUST track progress meticulously to survive compaction.
  </critical_warning>

  <work_tracking_directory>
    <path>oxs/.work/tdd/</path>
    <purpose>Persistent work state that survives context compaction</purpose>
    <critical>Create this directory FIRST before any other work</critical>

    <required_files>
      <file name="progress.yaml">
        <purpose>Track current phase, component, and test status</purpose>
        <updated>After EVERY test passes (green state)</updated>
        <critical>This is your lifeline for resumption</critical>
      </file>

      <file name="test-inventory.yaml">
        <purpose>Catalogue of all tests: planned, written, passing</purpose>
        <format>Test name, component, status, requirement traced</format>
      </file>

      <file name="implementation-log.yaml">
        <purpose>Log of what was implemented and when</purpose>
        <format>Timestamped entries of code written</format>
      </file>

      <file name="issues.yaml">
        <purpose>Track any issues, blockers, or technical debt</purpose>
        <format>Issue description, severity, resolution status</format>
      </file>
    </required_files>
  </work_tracking_directory>

  <progress_tracking_schema>
```yaml
# progress.yaml - UPDATE AFTER EVERY GREEN TEST
progress:
  last_updated: "[ISO DateTime]"
  current_phase: "[Phase ID]"
  current_component: "[Component name]"
  current_test: "[Test being worked on]"
  tdd_state: "Red | Green | Refactor"
  status: "In Progress | Blocked | Complete"

  phases:
    phase_1_project_setup:
      status: "Not Started | In Progress | Complete"
      csproj_created: false
      test_project_created: false
      can_build: false

    phase_2_core_domain:
      status: "Not Started | In Progress | Complete"
      components:
        PlayerId:
          tests_planned: 0
          tests_passing: 0
          implemented: false
        Board:
          tests_planned: 0
          tests_passing: 0
          implemented: false
        GameRules:
          tests_planned: 0
          tests_passing: 0
          implemented: false
        GameState:
          tests_planned: 0
          tests_passing: 0
          implemented: false

    phase_3_ai:
      status: "Not Started | In Progress | Complete"
      components:
        EasyAI:
          tests_planned: 0
          tests_passing: 0
          implemented: false
        MediumAI:
          tests_planned: 0
          tests_passing: 0
          implemented: false
        HardAI:
          tests_planned: 0
          tests_passing: 0
          implemented: false

    phase_4_presentation:
      status: "Not Started | In Progress | Complete"
      scenes_created: []
      scripts_attached: []

    phase_5_networking:
      status: "Not Started | In Progress | Complete"
      components: {}

    phase_6_integration:
      status: "Not Started | In Progress | Complete"
      modes_working:
        local_2player: false
        vs_ai: false
        networked: false

  test_summary:
    total_planned: 0
    total_written: 0
    total_passing: 0
    total_failing: 0

  next_action: "[EXACTLY what to do next - be specific]"
```
  </progress_tracking_schema>

  <resumption_protocol>
  WHEN CONTEXT IS COMPACTED OR SESSION RESUMES:

  1. IMMEDIATELY check for existing progress:
     ```bash
     cat oxs/.work/tdd/progress.yaml 2>/dev/null || echo "NO_PROGRESS_FILE"
     ```

  2. IF progress file exists:
     - Read current_phase, current_component, current_test
     - Read tdd_state (Red/Green/Refactor)
     - Read next_action CAREFULLY - it tells you exactly what to do
     - Run `dotnet test` to verify current state matches recorded state
     - Resume from next_action - do NOT restart from beginning

  3. IF no progress file (fresh start):
     - Verify all prerequisites exist
     - Initialize .work/tdd/ directory
     - Begin with Phase 1 (Project Setup)

  4. CHECKPOINT TRIGGERS (update progress.yaml):
     - After EVERY test goes green
     - Before starting a new component
     - After completing a refactor
     - Before any complex change
     - At natural stopping points
  </resumption_protocol>

  <compaction_safe_practices>
    <practice>Update progress.yaml after EVERY green test</practice>
    <practice>Write test-inventory.yaml before writing tests</practice>
    <practice>Document next_action with enough detail to resume cold</practice>
    <practice>Run `dotnet test` after resumption to verify state</practice>
    <practice>Complete one component fully before starting another</practice>
    <practice>Never leave in Red state at checkpoint</practice>
  </compaction_safe_practices>
</context_compaction_survival>

<tdd_methodology>
  <red_green_refactor>
  The TDD cycle is sacred. For EVERY piece of functionality:

  **RED Phase:**
  1. Write a single test for one specific behaviour
  2. Run the test - it MUST fail (if it passes, test is wrong)
  3. Verify the failure message makes sense

  **GREEN Phase:**
  1. Write the MINIMUM code to make the test pass
  2. Don't over-engineer - just make it work
  3. Run the test - it MUST pass
  4. Update progress.yaml immediately

  **REFACTOR Phase:**
  1. Look for opportunities to improve code
  2. Ensure STD-0002 compliance
  3. Run tests - they MUST still pass
  4. Only refactor when tests are green
  </red_green_refactor>

  <test_naming_convention>
  Follow this naming pattern (from STD-0002):

  ```
  [MethodName]_[Scenario]_[ExpectedResult]
  ```

  Examples:
  - `CheckResult_ThreeXsInRow_ReturnsXWins`
  - `WithMove_CellAlreadyOccupied_ThrowsInvalidOperation`
  - `SelectMove_EmptyBoard_ReturnsValidCell`
  </test_naming_convention>

  <test_structure>
  Use Arrange-Act-Assert (AAA) pattern:

  ```csharp
  [Fact]
  public void MethodName_Scenario_ExpectedResult() {
      // Arrange
      var board = Board.Empty;

      // Act
      var result = GameRules.CheckResult(board);

      // Assert
      result.Should().BeOfType<GameResult.InProgress>();
  }
  ```
  </test_structure>

  <test_coverage_requirements>
  For each component, tests must cover:

  - **Happy path** - Normal expected usage
  - **Edge cases** - Boundary conditions
  - **Error cases** - Invalid inputs, illegal states
  - **State transitions** - For stateful components

  Aim for:
  - 100% coverage of Core Domain
  - 100% coverage of AI algorithms
  - Integration tests for game modes
  </test_coverage_requirements>
</tdd_methodology>

<implementation_phases>
  <phase id="1" name="Project Setup">
    <description>Create project structure and verify build works</description>
    <tasks>
      <task>Create OXS.csproj in oxs/src/oxs/</task>
      <task>Create OXS.Tests.csproj in oxs/tests/OXS.Tests/</task>
      <task>Add project references</task>
      <task>Add FluentAssertions package to test project</task>
      <task>Create GlobalUsings.cs files</task>
      <task>Verify `dotnet build` succeeds</task>
      <task>Verify `dotnet test` runs (0 tests)</task>
    </tasks>
    <verification>
    ```bash
    cd oxs && dotnet build && dotnet test
    ```
    Must complete with no errors.
    </verification>
  </phase>

  <phase id="2" name="Core Domain">
    <description>Implement game logic with TDD - ZERO Godot dependencies</description>
    <order>
    Implement in dependency order:
    1. CellState (enum)
    2. PlayerId (record struct)
    3. Board (record)
    4. Move (record)
    5. GameResult (discriminated union)
    6. GameRules (static class)
    7. GameState (state machine)
    </order>
    <test_examples>
```csharp
// BoardTests.cs
public class BoardTests {
    [Fact]
    public void Empty_ReturnsEmptyBoard() {
        // Arrange & Act
        var board = Board.Empty;

        // Assert
        board.EmptyCells.Should().HaveCount(9);
    }

    [Fact]
    public void WithMove_ValidMove_ReturnsNewBoardWithMove() {
        // Arrange
        var board = Board.Empty;

        // Act
        var newBoard = board.WithMove(4, CellState.X);

        // Assert
        newBoard[4].Should().Be(CellState.X);
        board[4].Should().Be(CellState.Empty); // Original unchanged
    }

    [Fact]
    public void WithMove_OccupiedCell_ThrowsInvalidOperation() {
        // Arrange
        var board = Board.Empty.WithMove(0, CellState.X);

        // Act
        var act = () => board.WithMove(0, CellState.O);

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }
}

// GameRulesTests.cs
public class GameRulesTests {
    [Fact]
    public void CheckResult_EmptyBoard_ReturnsInProgress() {
        // Arrange
        var board = Board.Empty;

        // Act
        var result = GameRules.CheckResult(board);

        // Assert
        result.Should().BeOfType<GameResult.InProgress>();
    }

    [Theory]
    [InlineData(0, 1, 2)] // Top row
    [InlineData(3, 4, 5)] // Middle row
    [InlineData(6, 7, 8)] // Bottom row
    [InlineData(0, 3, 6)] // Left column
    [InlineData(0, 4, 8)] // Main diagonal
    public void CheckResult_ThreeXsInLine_ReturnsXWins(int a, int b, int c) {
        // Arrange
        var board = Board.Empty
            .WithMove(a, CellState.X)
            .WithMove(b, CellState.X)
            .WithMove(c, CellState.X);

        // Act
        var result = GameRules.CheckResult(board);

        // Assert
        result.Should().BeOfType<GameResult.Win>()
            .Which.Winner.Should().Be(PlayerId.Player1);
    }

    [Fact]
    public void CheckResult_FullBoardNoWinner_ReturnsDraw() {
        // Arrange - Classic draw position
        // X O X
        // X X O
        // O X O
        var board = Board.Empty
            .WithMove(0, CellState.X).WithMove(1, CellState.O).WithMove(2, CellState.X)
            .WithMove(3, CellState.X).WithMove(4, CellState.X).WithMove(5, CellState.O)
            .WithMove(6, CellState.O).WithMove(7, CellState.X).WithMove(8, CellState.O);

        // Act
        var result = GameRules.CheckResult(board);

        // Assert
        result.Should().BeOfType<GameResult.Draw>();
    }
}
```
    </test_examples>
  </phase>

  <phase id="3" name="AI Layer">
    <description>Implement AI opponents with TDD</description>
    <order>
    1. IAIPlayer interface
    2. EasyAI (random)
    3. MediumAI (heuristic)
    4. HardAI (minimax)
    5. AIFactory
    </order>
    <test_examples>
```csharp
// EasyAITests.cs
public class EasyAITests {
    [Fact]
    public void SelectMove_EmptyBoard_ReturnsValidCell() {
        // Arrange
        var ai = new EasyAI();
        var board = Board.Empty;

        // Act
        var move = ai.SelectMove(board, CellState.X);

        // Assert
        move.Should().BeInRange(0, 8);
        board[move].Should().Be(CellState.Empty);
    }

    [Fact]
    public void SelectMove_OneEmptyCell_ReturnsThatCell() {
        // Arrange
        var ai = new EasyAI();
        var board = Board.Empty
            .WithMove(0, CellState.X).WithMove(1, CellState.O)
            .WithMove(2, CellState.X).WithMove(3, CellState.O)
            .WithMove(4, CellState.X).WithMove(5, CellState.O)
            .WithMove(6, CellState.O).WithMove(7, CellState.X);
        // Only cell 8 is empty

        // Act
        var move = ai.SelectMove(board, CellState.O);

        // Assert
        move.Should().Be(8);
    }
}

// HardAITests.cs
public class HardAITests {
    [Fact]
    public void SelectMove_CanWinImmediately_TakesWinningMove() {
        // Arrange
        var ai = new HardAI();
        // X X _
        // O O _
        // _ _ _
        var board = Board.Empty
            .WithMove(0, CellState.X).WithMove(1, CellState.X)
            .WithMove(3, CellState.O).WithMove(4, CellState.O);

        // Act
        var move = ai.SelectMove(board, CellState.X);

        // Assert
        move.Should().Be(2); // Complete the win
    }

    [Fact]
    public void SelectMove_OpponentCanWin_BlocksThem() {
        // Arrange
        var ai = new HardAI();
        // X X _
        // O _ _
        // _ _ _
        var board = Board.Empty
            .WithMove(0, CellState.X).WithMove(1, CellState.X)
            .WithMove(3, CellState.O);

        // Act
        var move = ai.SelectMove(board, CellState.O);

        // Assert
        move.Should().Be(2); // Block the win
    }

    [Fact]
    public void SelectMove_OptimalPlay_NeverLoses() {
        // Arrange
        var ai = new HardAI();

        // Act & Assert - Play 100 games against random
        var random = new Random(42);
        for (var game = 0; game < 100; game++) {
            var board = Board.Empty;
            var aiIsX = game % 2 == 0;

            while (true) {
                var result = GameRules.CheckResult(board);
                if (result is not GameResult.InProgress) {
                    // AI should never lose
                    if (aiIsX) {
                        result.Should().NotBeOfType<GameResult.Win>(
                            because: "Hard AI as X should never lose")
                            .Or.Match<GameResult.Win>(w => w.Winner == PlayerId.Player1);
                    } else {
                        result.Should().NotBeOfType<GameResult.Win>(
                            because: "Hard AI as O should never lose")
                            .Or.Match<GameResult.Win>(w => w.Winner == PlayerId.Player2);
                    }
                    break;
                }

                var isAiTurn = (board.EmptyCells.Count() % 2 == 1) == aiIsX;
                if (isAiTurn) {
                    var move = ai.SelectMove(board, aiIsX ? CellState.X : CellState.O);
                    board = board.WithMove(move, aiIsX ? CellState.X : CellState.O);
                } else {
                    // Random opponent move
                    var emptyCells = board.EmptyCells.ToList();
                    var move = emptyCells[random.Next(emptyCells.Count)];
                    board = board.WithMove(move, aiIsX ? CellState.O : CellState.X);
                }
            }
        }
    }
}
```
    </test_examples>
  </phase>

  <phase id="4" name="Presentation Layer">
    <description>Create Godot scenes and scripts</description>
    <note>
    This phase involves Godot editor work which is harder to test.
    Focus on:
    - Creating scene files (.tscn)
    - Attaching scripts
    - Connecting signals
    - Manual testing of UI
    </note>
    <tasks>
      <task>Create Main.tscn (main menu)</task>
      <task>Create Board.tscn (game board scene)</task>
      <task>Create Cell.tscn (individual cell)</task>
      <task>Implement BoardView.cs</task>
      <task>Implement CellView.cs</task>
      <task>Implement GameController.cs</task>
      <task>Wire up signals</task>
      <task>Test local 2-player manually</task>
      <task>Test AI opponent manually</task>
    </tasks>
  </phase>

  <phase id="5" name="Networking Layer">
    <description>Implement networked multiplayer</description>
    <tasks>
      <task>Create NetworkManager.cs (Autoload)</task>
      <task>Define message types</task>
      <task>Implement LobbyManager.cs</task>
      <task>Implement GameSynchroniser.cs</task>
      <task>Test connection flow</task>
      <task>Test state synchronisation</task>
      <task>Test disconnection handling</task>
    </tasks>
    <testing_note>
    Network testing requires two game instances.
    Create a simple test harness or use manual testing.
    Document test scenarios in test-inventory.yaml.
    </testing_note>
  </phase>

  <phase id="6" name="Integration">
    <description>Verify all game modes work end-to-end</description>
    <verification>
      <mode name="Local 2-Player">
        - Two players can take turns
        - Win detection works
        - Draw detection works
        - Rematch works
      </mode>
      <mode name="AI Opponent">
        - All difficulty levels work
        - Easy AI is beatable
        - Hard AI never loses
        - Turn alternation correct
      </mode>
      <mode name="Networked">
        - Players can connect
        - Moves synchronise
        - Disconnection handled
        - Reconnection works
      </mode>
    </verification>
  </phase>
</implementation_phases>

<output_specifications>
  <primary_outputs>
    <output path="oxs/src/oxs/">Production code</output>
    <output path="oxs/tests/OXS.Tests/">Test code</output>
  </primary_outputs>

  <project_structure>
```
oxs/
├── src/
│   └── oxs/
│       ├── project.godot           # (exists)
│       ├── OXS.csproj              # Main project
│       ├── GlobalUsings.cs         # Global using statements
│       ├── Core/
│       │   ├── CellState.cs
│       │   ├── PlayerId.cs
│       │   ├── Board.cs
│       │   ├── Move.cs
│       │   ├── GameResult.cs
│       │   ├── GameRules.cs
│       │   ├── GameState.cs
│       │   └── AI/
│       │       ├── IAIPlayer.cs
│       │       ├── EasyAI.cs
│       │       ├── MediumAI.cs
│       │       ├── HardAI.cs
│       │       └── AIFactory.cs
│       ├── Presentation/
│       │   ├── Scenes/
│       │   │   ├── Main.tscn
│       │   │   ├── Board.tscn
│       │   │   └── Cell.tscn
│       │   ├── BoardView.cs
│       │   ├── CellView.cs
│       │   ├── GameController.cs
│       │   ├── UIManager.cs
│       │   └── AudioManager.cs
│       └── Networking/
│           ├── NetworkManager.cs
│           ├── LobbyManager.cs
│           ├── GameSynchroniser.cs
│           └── Messages/
│               ├── INetworkMessage.cs
│               ├── MoveMessage.cs
│               └── StateMessage.cs
│
└── tests/
    └── OXS.Tests/
        ├── OXS.Tests.csproj
        ├── GlobalUsings.cs
        ├── Core/
        │   ├── BoardTests.cs
        │   ├── GameRulesTests.cs
        │   ├── GameStateTests.cs
        │   └── AI/
        │       ├── EasyAITests.cs
        │       ├── MediumAITests.cs
        │       └── HardAITests.cs
        └── Integration/
            └── GameFlowTests.cs
```
  </project_structure>

  <csproj_templates>
```xml
<!-- oxs/src/oxs/OXS.csproj -->
<Project Sdk="Godot.NET.Sdk/4.5.0">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <RootNamespace>OXS</RootNamespace>
  </PropertyGroup>
</Project>
```

```xml
<!-- oxs/tests/OXS.Tests/OXS.Tests.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.*" />
    <PackageReference Include="xunit" Version="2.*" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.*">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    <PackageReference Include="FluentAssertions" Version="6.*" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="../../src/oxs/OXS.csproj" />
  </ItemGroup>
</Project>
```
  </csproj_templates>
</output_specifications>

<critical_reminders>
================================================================================
                    CRITICAL REMINDERS
================================================================================

1. **TDD IS MANDATORY**
   - Write test FIRST, then implementation
   - Never write production code without failing test
   - If test passes immediately, the test is wrong

2. **CHECKPOINT AFTER EVERY GREEN**
   - Update progress.yaml immediately when test passes
   - Document next_action clearly
   - This is how you survive compaction

3. **STD-0002 COMPLIANCE**
   - All code follows the rubric
   - Egyptian braces, records, pattern matching
   - Verify code style as you write

4. **CORE HAS ZERO GODOT**
   - No `using Godot;` in Core namespace
   - Test Core with `dotnet test`
   - This is non-negotiable

5. **VERIFY PREREQUISITES**
   - Requirements, GDD, AND Architecture must exist
   - Read Architecture before implementing
   - Implementation must match Architecture

6. **COMPLETE IN ORDER**
   - Phase 1 → Phase 2 → Phase 3 → Phase 4 → Phase 5 → Phase 6
   - Within phases, follow component order
   - Don't skip ahead

7. **RUN TESTS FREQUENTLY**
   ```bash
   cd oxs && dotnet test
   ```
   Run after every change. All tests must pass.

8. **NEVER LEAVE IN RED STATE**
   - If you must stop, get to green first
   - If stuck, document the issue in issues.yaml
   - Checkpoints only at green or refactor complete

</critical_reminders>

<begin>
=====================================
CRITICAL: CHECK FOR EXISTING PROGRESS FIRST
=====================================
This TDD implementation will span multiple context compactions.

FIRST ACTION - Check for existing progress:
```bash
cat oxs/.work/tdd/progress.yaml 2>/dev/null || echo "NO_PROGRESS_FILE"
```

IF progress file exists:
- Read current_phase, current_component, current_test
- Read tdd_state (Red/Green/Refactor)
- Read next_action CAREFULLY
- Run `dotnet test` to verify state
- Resume from next_action

IF no progress file (fresh start):
- Verify all prerequisites exist
- Create oxs/.work/tdd/ directory
- Begin with Phase 1 (Project Setup)

=====================================
STARTING WORKFLOW
=====================================

1. **Verify prerequisites**
   ```bash
   ls -la oxs/docs/requirements/OXS-REQUIREMENTS.md \
          oxs/docs/design/OXS-GDD.md \
          oxs/docs/architecture/OXS-ARCHITECTURE.md 2>/dev/null
   ```
   IF any missing: STOP and inform user which prompt to run first

2. **Read Architecture document**
   Understand the components, interfaces, and structure before coding

3. **Phase 1: Project Setup**
   - Create OXS.csproj
   - Create OXS.Tests.csproj
   - Verify build works

4. **Phase 2: Core Domain (TDD)**
   For each component in order:
   a. Plan tests (write to test-inventory.yaml)
   b. Write first test (RED)
   c. Run test - verify it fails
   d. Write minimum code (GREEN)
   e. Run test - verify it passes
   f. Update progress.yaml
   g. Refactor if needed
   h. Repeat for next test

5. **Phase 3: AI Layer (TDD)**
   Same TDD cycle for AI components

6. **Phase 4: Presentation**
   Create Godot scenes and scripts
   Manual testing for UI

7. **Phase 5: Networking**
   Implement network layer
   Test with two instances

8. **Phase 6: Integration**
   Verify all modes work end-to-end

=====================================
REMEMBER: UPDATE progress.yaml AFTER EVERY GREEN TEST
=====================================

BEGIN NOW by checking for existing progress.
</begin>
