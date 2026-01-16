# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Test Commands

```bash
# Build the solution
cd oxs && dotnet build

# Run all tests
cd oxs && dotnet test

# Run specific test class
cd oxs && dotnet test --filter "ClassName"

# Run specific test method
cd oxs && dotnet test --filter "FullyQualifiedName~TestMethodName"

# Open in Godot Editor
godot --path oxs/src/oxs --editor
```

## Architecture Overview

This is **OXS** (Noughts & Crosses/Tic-Tac-Toe), a Godot 4.5.1 game using C# 12 / .NET 8 with strict TDD methodology.

### Layer Architecture (Dependencies Flow Downward)

```
Presentation Layer (Godot nodes, scenes, UI)
        ↓
   Networking Layer (WebRTC/ENet multiplayer)
        ↓
      AI Layer (Easy, Medium, Hard, Adaptive)
        ↓
   Core Domain (Pure C#, ZERO Godot dependencies)
```

### Key Design Principle: Core Has Zero Godot Dependencies

The `Core/` layer contains all game logic as pure C# with no Godot references. This enables:
- Unit tests run without Godot runtime (~10x faster)
- CI/CD with standard .NET tooling
- Clear architectural boundary

### Project Structure

```
oxs/
├── src/oxs/
│   ├── Core/              # Pure C# game logic (Board, GameRules, GameStateMachine)
│   │   └── AI/            # AI implementations (Easy, Medium, Hard, Adaptive)
│   ├── Presentation/      # Godot scenes/scripts (BoardView, CellView, GameController)
│   │   ├── Scenes/        # .tscn scene files
│   │   ├── Shaders/       # GLSL shaders (glassmorphism, gradients)
│   │   └── Theme/         # UI theming
│   └── Networking/        # Multiplayer (WebRTC signaling, transports)
├── tests/OXS.Tests/       # xUnit + FluentAssertions tests
└── docs/                  # Architecture, GDD, requirements
```

## Code Style (STD-0002)

**Mandatory conventions:**

1. **Egyptian braces** (cuddled) - never Allman style
2. **No XML docs** - code is self-documenting via naming
3. **No magic strings** - use enums, constants, `nameof()`
4. **Strongly-typed IDs** - `PlayerId` not `int`
5. **Result pattern** - no exceptions for expected failures
6. **Immutability** - records with `with` expressions, `ImmutableArray<T>`
7. **Early returns** - avoid deep nesting (max 2 levels)
8. **Pattern matching** - switch expressions over statements

**Example (correct style):**
```csharp
public sealed record PlayerId(int Value) {
    public static readonly PlayerId X = new(0);
    public static readonly PlayerId O = new(1);
    public PlayerId Opponent => Value == 0 ? O : X;
}
```

## Key Files Reference

| Purpose | Location |
|---------|----------|
| Architecture design | `oxs/docs/architecture/OXS-ARCHITECTURE.md` |
| Game design document | `oxs/docs/design/OXS-GDD.md` |
| Requirements | `oxs/docs/requirements/OXS-REQUIREMENTS.md` |
| C# style rubric | `oxs/prompts/rubrics/STD-0002-csharp-rubric.md` |
| Work progress | `oxs/.work/tdd/progress.yaml` |

## Core Types

- `Board` - Immutable board state using `ImmutableArray<CellState>`
- `GameStateMachine` - State management with events (`StateChanged`, `MoveMade`, `GameEnded`)
- `GameRules` - Static pure functions for win detection, valid moves
- `Result<T>` - Discriminated union for error handling (`Success`/`Failure`)
- `GameResult` - Win/Draw/InProgress union with winning line data
- `IAIPlayer` - AI interface with `SelectMove(Board, PlayerId, int winLength)`

## Godot-Specific Patterns

- Use `[Signal]` delegates for node communication
- Use `%UniqueNames` for node references (not path strings)
- `_Ready()` for initialization, never C# constructors
- `partial` keyword required on all Node-derived classes
