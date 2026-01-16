# OXS-ARCH: Architecture Design Prompt

<context>
  <project>Friday Games - OXS (Noughts & Crosses)</project>
  <role>Software Architect specialising in game development with Godot 4.5 and C#</role>
  <objective>Design a clean, testable, and maintainable architecture that separates core game logic from Godot-specific code, following the STD-0002 C# code style rubric</objective>
  <sequence>Prompt 3 of 4 in the OXS development pipeline</sequence>
  <references>
    <doc path="oxs/prompts/rubrics/STD-0001-prompt-creation-rubric.md">Prompt creation patterns</doc>
    <doc path="oxs/prompts/rubrics/STD-0002-csharp-rubric.md" critical="true">C# code style guidelines - MUST follow</doc>
  </references>
  <input_documents>
    <doc path="oxs/docs/requirements/OXS-REQUIREMENTS.md" required="true">Requirements specification</doc>
    <doc path="oxs/docs/design/OXS-GDD.md" required="true">Game Design Document</doc>
  </input_documents>
</context>

<foundational_principles>
1. **Core has ZERO Godot dependencies** - Game logic in pure C# for testability
2. **Presentation depends on Core, never reverse** - One-way dependency
3. **Behaviour emerges from systems** - Not scripts telling the computer what to do
4. **Minimal code, maximum clarity** - If you can delete code without losing capability, do it
5. **Strongly-typed everything** - No magic strings, use enums and typed IDs
6. **Functional paradigm preferred** - Immutability, pure functions, composition
7. **Small components** - Many small files over few large ones
</foundational_principles>

<std_0002_compliance>
  <critical_warning>
  ALL architecture decisions MUST align with STD-0002-csharp-rubric.md.
  Read and internalise the rubric before designing.
  Every code example in this document MUST follow rubric conventions.
  </critical_warning>

  <key_patterns_from_rubric>
    <pattern name="Egyptian braces">Cuddled braces for ALL code blocks</pattern>
    <pattern name="Records for value objects">Immutable data with `record` types</pattern>
    <pattern name="Strongly-typed IDs">`PlayerId`, `GameId` not raw `int`</pattern>
    <pattern name="Result pattern">No exceptions for expected failures</pattern>
    <pattern name="Pattern matching">Switch expressions, property patterns</pattern>
    <pattern name="No magic strings">Enums, constants, `nameof()`</pattern>
    <pattern name="Rich entities">Behaviour in domain objects, not services</pattern>
    <pattern name="Early returns">Flat code, max 2 levels nesting</pattern>
    <pattern name="Primary constructors">For services with dependencies</pattern>
    <pattern name="Godot signals">For decoupled node communication</pattern>
  </key_patterns_from_rubric>
</std_0002_compliance>

<context_compaction_survival>
  <critical_warning>
  This architecture work may span multiple context compactions.
  Architecture decisions must be persisted to survive compaction.
  </critical_warning>

  <work_tracking_directory>
    <path>oxs/.work/architecture/</path>
    <purpose>Persistent work state that survives context compaction</purpose>
    <critical>Create this directory FIRST before any other work</critical>

    <required_files>
      <file name="progress.yaml">
        <purpose>Track which architecture sections are complete</purpose>
        <updated>After EVERY section completed</updated>
      </file>

      <file name="design-decisions.yaml">
        <purpose>Architectural Decision Records (lightweight)</purpose>
        <format>Decision, context, rationale, consequences</format>
      </file>

      <file name="component-inventory.yaml">
        <purpose>Catalogue of all designed components</purpose>
        <format>Component name, layer, responsibility, dependencies</format>
      </file>

      <directory name="section-drafts/">
        <purpose>Working drafts of each architecture section</purpose>
        <format>One .md file per major section</format>
      </directory>
    </required_files>
  </work_tracking_directory>

  <progress_tracking_schema>
```yaml
# progress.yaml - UPDATE AFTER EVERY SECTION
progress:
  last_updated: "[ISO DateTime]"
  current_section: "[Section ID]"
  status: "In Progress | Blocked | Complete"

  sections:
    layer_overview:
      status: "Not Started | In Progress | Complete"
    core_domain:
      status: "Not Started | In Progress | Complete"
    ai_layer:
      status: "Not Started | In Progress | Complete"
    presentation_layer:
      status: "Not Started | In Progress | Complete"
    networking_layer:
      status: "Not Started | In Progress | Complete"
    integration:
      status: "Not Started | In Progress | Complete"

  components_designed: 0
  interfaces_defined: 0
  signals_defined: 0

  next_action: "[Exactly what to do next]"
```
  </progress_tracking_schema>

  <resumption_protocol>
  WHEN CONTEXT IS COMPACTED OR SESSION RESUMES:

  1. IMMEDIATELY check for existing progress:
     ```bash
     cat oxs/.work/architecture/progress.yaml 2>/dev/null || echo "NO_PROGRESS_FILE"
     ```

  2. IF progress file exists:
     - Read current_section, next_action
     - Review section-drafts/ for completed work
     - Review component-inventory.yaml for designed components
     - Resume from next_action

  3. IF no progress file:
     - Verify prerequisites (requirements + GDD exist)
     - Initialize .work/architecture/ directory
     - Begin with Layer Overview section
  </resumption_protocol>
</context_compaction_survival>

<architectural_layers>
  <layer id="core" name="Core Domain">
    <location>oxs/src/oxs/Core/</location>
    <dependencies>NONE - pure C#, zero Godot references</dependencies>
    <responsibility>
      Game rules, state management, AI algorithms.
      Must be fully testable without Godot runtime.
    </responsibility>
    <components>
      <component name="Board">Board state representation (record)</component>
      <component name="GameRules">Win/draw detection, move validation</component>
      <component name="GameState">Current game state machine</component>
      <component name="Player">Player representation with typed ID</component>
      <component name="Move">Move representation (record)</component>
    </components>
  </layer>

  <layer id="ai" name="AI Layer">
    <location>oxs/src/oxs/Core/AI/</location>
    <dependencies>Core Domain only</dependencies>
    <responsibility>
      AI opponent implementations. Part of Core because
      AI is pure computation with no Godot dependencies.
    </responsibility>
    <components>
      <component name="IAIPlayer">Interface for AI players</component>
      <component name="EasyAI">Random move selection</component>
      <component name="MediumAI">Heuristic-based (block/take wins)</component>
      <component name="HardAI">Minimax with alpha-beta pruning</component>
      <component name="AIFactory">Creates AI by difficulty level</component>
    </components>
  </layer>

  <layer id="presentation" name="Presentation Layer">
    <location>oxs/src/oxs/Presentation/</location>
    <dependencies>Core Domain, Godot</dependencies>
    <responsibility>
      Godot nodes, scenes, UI, input handling.
      Translates Core events into visual/audio feedback.
    </responsibility>
    <components>
      <component name="BoardView">Visual representation of the board (Node2D)</component>
      <component name="CellView">Individual cell display and input</component>
      <component name="GameController">Orchestrates game flow (Godot node)</component>
      <component name="UIManager">Menu and HUD management</component>
      <component name="AudioManager">Sound effect playback</component>
    </components>
  </layer>

  <layer id="networking" name="Networking Layer">
    <location>oxs/src/oxs/Networking/</location>
    <dependencies>Core Domain, Godot (for networking APIs)</dependencies>
    <responsibility>
      Network communication, state synchronisation,
      lobby management, connection handling.
    </responsibility>
    <components>
      <component name="NetworkManager">Connection lifecycle (Autoload)</component>
      <component name="GameSynchroniser">Board state sync</component>
      <component name="LobbyManager">Host/join flow</component>
      <component name="MessageSerializer">Network message encoding</component>
    </components>
  </layer>
</architectural_layers>

<design_methodology>
  <approach>
  Design from the inside out:

  1. **Core Domain First** - Define the pure game logic
  2. **AI as Extension** - AI uses Core but remains pure
  3. **Presentation Wraps Core** - Nodes observe and update Core
  4. **Networking Synchronises Core** - Network layer syncs Core state
  </approach>

  <interface_design>
  Define clear interfaces between layers:

  - Core exposes: Immutable state, pure functions, events/callbacks
  - Presentation consumes: State for rendering, events for feedback
  - Networking serialises: State for transmission, commands for actions

  Use C# events or callbacks in Core (not Godot signals) for layer decoupling.
  Use Godot signals only within Presentation layer.
  </interface_design>

  <state_ownership>
  Core Domain OWNS the authoritative game state:

  - Presentation READS state, SENDS commands
  - Networking READS state, TRANSMITS state, RECEIVES commands
  - AI READS state, COMPUTES moves

  Core validates all state changes. Invalid moves are rejected with Result.
  </state_ownership>
</design_methodology>

<output_specifications>
  <primary_output>
    <file>oxs/docs/architecture/OXS-ARCHITECTURE.md</file>
    <format>Markdown with diagrams and code examples</format>
  </primary_output>

  <document_structure>
```markdown
# OXS Architecture Design Document

## Document Information
- **Version:** 1.0
- **Date:** [Date]
- **Status:** Draft | Under Review | Approved
- **Based on:** OXS-REQUIREMENTS.md, OXS-GDD.md
- **Code Style:** STD-0002-csharp-rubric.md

## Table of Contents
1. Architecture Overview
2. Layer Design
3. Core Domain
4. AI Layer
5. Presentation Layer
6. Networking Layer
7. Component Catalogue
8. Interface Specifications
9. Signal Definitions
10. Project Structure
11. Appendices

---

## 1. Architecture Overview

### 1.1 Architectural Goals
- Testable core logic (zero Godot dependencies)
- Clear separation of concerns
- Minimal code following STD-0002
- Support for all three game modes

### 1.2 Layer Diagram
```
┌─────────────────────────────────────────────────────────┐
│                    Presentation Layer                    │
│  (Godot Nodes, Scenes, UI, Input, Audio)                │
├─────────────────────────────────────────────────────────┤
│                    Networking Layer                      │
│  (Connection, Sync, Lobby, Messages)                    │
├─────────────────────────────────────────────────────────┤
│                    AI Layer                              │
│  (Easy, Medium, Hard AI implementations)                │
├─────────────────────────────────────────────────────────┤
│                    Core Domain                           │
│  (Board, Rules, State, Player, Move)                    │
│  *** ZERO GODOT DEPENDENCIES ***                        │
└─────────────────────────────────────────────────────────┘
```

### 1.3 Dependency Rules
- Core Domain: No dependencies (pure C#)
- AI Layer: Depends only on Core Domain
- Networking Layer: Depends on Core Domain + Godot networking
- Presentation Layer: Depends on Core Domain + Godot

---

## 2. Layer Design

[Detailed layer descriptions with responsibilities]

---

## 3. Core Domain

### 3.1 Types

#### PlayerId
```csharp
public readonly record struct PlayerId(int Value) : IEquatable<PlayerId> {
    public static PlayerId Player1 => new(1);
    public static PlayerId Player2 => new(2);
    public static PlayerId None => new(0);

    public override string ToString() => Value switch {
        1 => "X",
        2 => "O",
        _ => "-"
    };
}
```

#### CellState
```csharp
public enum CellState {
    Empty = 0,
    X = 1,
    O = 2
}
```

#### Board
```csharp
public sealed record Board {
    private readonly CellState[] _cells = new CellState[9];

    public CellState this[int index] => _cells[index];
    public CellState this[int row, int col] => _cells[row * 3 + col];

    public static Board Empty => new();

    public Board WithMove(int index, CellState state) {
        if (index < 0 || index > 8) {
            throw new ArgumentOutOfRangeException(nameof(index));
        }
        if (_cells[index] != CellState.Empty) {
            throw new InvalidOperationException("Cell not empty");
        }

        var newCells = (CellState[])_cells.Clone();
        newCells[index] = state;

        return new Board { _cells = newCells };
    }

    public IEnumerable<int> EmptyCells =>
        Enumerable.Range(0, 9).Where(i => _cells[i] == CellState.Empty);

    public bool IsFull => !_cells.Contains(CellState.Empty);
}
```

### 3.2 Game Rules
[GameRules class with win detection]

### 3.3 Game State Machine
[State machine with transitions]

---

## 4. AI Layer

### 4.1 AI Interface
```csharp
public interface IAIPlayer {
    int SelectMove(Board board, CellState aiSymbol);
}
```

### 4.2 Implementations
[Easy, Medium, Hard AI implementations]

---

## 5. Presentation Layer

### 5.1 Scene Structure
[Scene hierarchy]

### 5.2 Node Classes
[BoardView, CellView, GameController]

### 5.3 Signals
[Signal definitions]

---

## 6. Networking Layer

### 6.1 Network Architecture
[Host-client model]

### 6.2 Message Types
[Network message definitions]

### 6.3 State Synchronisation
[Sync protocol]

---

## 7. Component Catalogue

| Component | Layer | File | Responsibility |
|-----------|-------|------|----------------|
| Board | Core | Core/Board.cs | Board state representation |
| GameRules | Core | Core/GameRules.cs | Win/draw detection |
| ... | ... | ... | ... |

---

## 8. Interface Specifications

### 8.1 Core Interfaces
[Interface definitions]

### 8.2 Layer Boundaries
[How layers communicate]

---

## 9. Signal Definitions

| Signal | Emitter | Payload | Subscribers |
|--------|---------|---------|-------------|
| MoveRequested | CellView | int cellIndex | GameController |
| GameStateChanged | GameController | GameState | BoardView, UIManager |
| ... | ... | ... | ... |

---

## 10. Project Structure

```
oxs/src/oxs/
├── OXS.csproj
├── Core/
│   ├── Board.cs
│   ├── CellState.cs
│   ├── GameRules.cs
│   ├── GameState.cs
│   ├── Move.cs
│   ├── PlayerId.cs
│   └── AI/
│       ├── IAIPlayer.cs
│       ├── EasyAI.cs
│       ├── MediumAI.cs
│       ├── HardAI.cs
│       └── AIFactory.cs
├── Presentation/
│   ├── Scenes/
│   │   ├── Main.tscn
│   │   ├── Board.tscn
│   │   └── Cell.tscn
│   ├── BoardView.cs
│   ├── CellView.cs
│   ├── GameController.cs
│   ├── UIManager.cs
│   └── AudioManager.cs
└── Networking/
    ├── NetworkManager.cs
    ├── GameSynchroniser.cs
    ├── LobbyManager.cs
    └── Messages/
        ├── INetworkMessage.cs
        ├── MoveMessage.cs
        └── StateMessage.cs

oxs/tests/OXS.Tests/
├── OXS.Tests.csproj
├── Core/
│   ├── BoardTests.cs
│   ├── GameRulesTests.cs
│   └── AI/
│       ├── EasyAITests.cs
│       ├── MediumAITests.cs
│       └── HardAITests.cs
└── GlobalUsings.cs
```

---

## 11. Appendices

### A. Architectural Decision Records
### B. STD-0002 Compliance Checklist
### C. Glossary
### D. Revision History
```
  </document_structure>
</output_specifications>

<examples>
  <example name="Core domain type (STD-0002 compliant)">
```csharp
// Core/Move.cs
namespace OXS.Core;

public sealed record Move(int CellIndex, PlayerId Player) {
    public static Result<Move> Create(int cellIndex, PlayerId player, Board board) {
        if (cellIndex < 0 || cellIndex > 8) {
            return Result<Move>.Fail("Cell index out of range");
        }

        if (board[cellIndex] != CellState.Empty) {
            return Result<Move>.Fail("Cell already occupied");
        }

        if (player == PlayerId.None) {
            return Result<Move>.Fail("Invalid player");
        }

        return Result<Move>.Ok(new Move(cellIndex, player));
    }
}
```
  </example>

  <example name="Game rules (STD-0002 compliant)">
```csharp
// Core/GameRules.cs
namespace OXS.Core;

public static class GameRules {
    private static readonly int[][] WinLines = [
        [0, 1, 2], [3, 4, 5], [6, 7, 8],  // Rows
        [0, 3, 6], [1, 4, 7], [2, 5, 8],  // Columns
        [0, 4, 8], [2, 4, 6]              // Diagonals
    ];

    public static GameResult CheckResult(Board board) {
        foreach (var line in WinLines) {
            var first = board[line[0]];
            if (first == CellState.Empty) {
                continue;
            }

            if (board[line[1]] == first && board[line[2]] == first) {
                return new GameResult.Win(first == CellState.X ? PlayerId.Player1 : PlayerId.Player2, line);
            }
        }

        return board.IsFull
            ? new GameResult.Draw()
            : new GameResult.InProgress();
    }
}

public abstract record GameResult {
    public sealed record Win(PlayerId Winner, int[] WinningLine) : GameResult;
    public sealed record Draw : GameResult;
    public sealed record InProgress : GameResult;
}
```
  </example>

  <example name="AI interface and implementation">
```csharp
// Core/AI/IAIPlayer.cs
namespace OXS.Core.AI;

public interface IAIPlayer {
    int SelectMove(Board board, CellState symbol);
}

// Core/AI/HardAI.cs
namespace OXS.Core.AI;

public sealed class HardAI : IAIPlayer {
    public int SelectMove(Board board, CellState symbol) {
        var bestScore = int.MinValue;
        var bestMove = -1;
        var opponent = symbol == CellState.X ? CellState.O : CellState.X;

        foreach (var cell in board.EmptyCells) {
            var newBoard = board.WithMove(cell, symbol);
            var score = Minimax(newBoard, 0, false, symbol, opponent, int.MinValue, int.MaxValue);

            if (score > bestScore) {
                bestScore = score;
                bestMove = cell;
            }
        }

        return bestMove;
    }

    private static int Minimax(
        Board board,
        int depth,
        bool isMaximising,
        CellState aiSymbol,
        CellState humanSymbol,
        int alpha,
        int beta) {

        var result = GameRules.CheckResult(board);

        return result switch {
            GameResult.Win w when GetSymbol(w.Winner) == aiSymbol => 10 - depth,
            GameResult.Win => depth - 10,
            GameResult.Draw => 0,
            _ => isMaximising
                ? MaxValue(board, depth, aiSymbol, humanSymbol, alpha, beta)
                : MinValue(board, depth, aiSymbol, humanSymbol, alpha, beta)
        };
    }

    private static int MaxValue(
        Board board,
        int depth,
        CellState aiSymbol,
        CellState humanSymbol,
        int alpha,
        int beta) {

        var maxEval = int.MinValue;

        foreach (var cell in board.EmptyCells) {
            var newBoard = board.WithMove(cell, aiSymbol);
            var eval = Minimax(newBoard, depth + 1, false, aiSymbol, humanSymbol, alpha, beta);
            maxEval = Math.Max(maxEval, eval);
            alpha = Math.Max(alpha, eval);

            if (beta <= alpha) {
                break;
            }
        }

        return maxEval;
    }

    private static int MinValue(
        Board board,
        int depth,
        CellState aiSymbol,
        CellState humanSymbol,
        int alpha,
        int beta) {

        var minEval = int.MaxValue;

        foreach (var cell in board.EmptyCells) {
            var newBoard = board.WithMove(cell, humanSymbol);
            var eval = Minimax(newBoard, depth + 1, true, aiSymbol, humanSymbol, alpha, beta);
            minEval = Math.Min(minEval, eval);
            beta = Math.Min(beta, eval);

            if (beta <= alpha) {
                break;
            }
        }

        return minEval;
    }

    private static CellState GetSymbol(PlayerId player) => player == PlayerId.Player1
        ? CellState.X
        : CellState.O;
}
```
  </example>

  <example name="Godot presentation node">
```csharp
// Presentation/BoardView.cs
namespace OXS.Presentation;

using Godot;
using OXS.Core;

public partial class BoardView : Node2D {
    [Signal] public delegate void CellClickedEventHandler(int cellIndex);

    private readonly CellView[] _cells = new CellView[9];

    public override void _Ready() {
        for (var i = 0; i < 9; i++) {
            _cells[i] = GetNode<CellView>($"%Cell{i}");
            var index = i;
            _cells[i].Clicked += () => EmitSignal(SignalName.CellClicked, index);
        }
    }

    public void UpdateFromBoard(Board board) {
        for (var i = 0; i < 9; i++) {
            _cells[i].SetState(board[i]);
        }
    }

    public void HighlightWinningLine(int[] line) {
        foreach (var index in line) {
            _cells[index].Highlight();
        }
    }
}
```
  </example>
</examples>

<critical_reminders>
================================================================================
                    CRITICAL REMINDERS
================================================================================

1. **READ STD-0002 FIRST**
   - All code examples must follow the rubric
   - Egyptian braces, no XML comments, pattern matching
   - Records for value objects, strongly-typed IDs

2. **CORE HAS ZERO GODOT DEPENDENCIES**
   - Board, GameRules, AI are pure C#
   - Use `System` namespaces only in Core
   - No `using Godot;` in Core layer

3. **VERIFY PREREQUISITES**
   - Requirements AND GDD must exist
   - If missing, STOP and inform user

4. **CHECKPOINT FREQUENTLY**
   - Update progress.yaml after each section
   - Log architectural decisions with rationale
   - Save component inventory as you design

5. **IMPLEMENTATION-READY**
   - Code examples compile (conceptually)
   - Interface contracts are complete
   - Project structure is concrete

6. **OUTPUT GOES TO docs/, NOT .work/**
   - .work/ is for internal tracking only
   - Final document goes to oxs/docs/architecture/
   - This is a deliverable for human review

</critical_reminders>

<begin>
=====================================
CRITICAL: CHECK FOR EXISTING PROGRESS FIRST
=====================================
This architecture work may have been started before context compaction.

FIRST ACTION - Check for existing progress:
```bash
cat oxs/.work/architecture/progress.yaml 2>/dev/null || echo "NO_PROGRESS_FILE"
```

IF progress file exists:
- Read current_section, next_action
- Review section-drafts/ for completed work
- Resume from where you left off

IF no progress file (fresh start):
- Verify prerequisites exist
- Create oxs/.work/architecture/ directory
- Read STD-0002 rubric first
- Begin with Architecture Overview

=====================================
STARTING WORKFLOW
=====================================

1. **Verify prerequisites**
   ```bash
   ls -la oxs/docs/requirements/OXS-REQUIREMENTS.md oxs/docs/design/OXS-GDD.md 2>/dev/null
   ```
   IF either missing: STOP and inform user

2. **Read STD-0002 rubric**
   ```bash
   cat oxs/prompts/rubrics/STD-0002-csharp-rubric.md
   ```
   Internalise the patterns before designing

3. **Design each section in order:**
   - Architecture Overview (layers, dependencies)
   - Core Domain (types, rules, state)
   - AI Layer (interface, implementations)
   - Presentation Layer (nodes, scenes, signals)
   - Networking Layer (connection, sync, messages)
   - Integration (how layers connect)

4. **For each section:**
   - Write detailed architecture
   - Include compliant code examples
   - Define interfaces and contracts
   - Save draft to section-drafts/
   - Update progress.yaml

5. **After all sections complete:**
   - Assemble final OXS-ARCHITECTURE.md
   - Generate component catalogue
   - Define signal table
   - Create project structure
   - Mark complete

BEGIN NOW by checking for existing progress, then verifying prerequisites.
</begin>
