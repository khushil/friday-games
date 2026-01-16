# OXS Software Architecture Specification

## Document Information

- **Version:** 1.0
- **Date:** 2026-01-16
- **Status:** Draft
- **Prerequisite Documents:** OXS-REQUIREMENTS.md, OXS-GDD.md, STD-0002

---

## 1. Introduction

### 1.1 Purpose

This document defines the software architecture for OXS, a noughts and crosses game developed with Godot 4.5 and C# (.NET 8). It provides the structural foundation for implementation, ensuring the system is testable, maintainable, and aligned with project requirements.

### 1.2 Scope

The architecture covers:

- Core game logic (board state, rules, win detection)
- AI opponent system (Easy, Medium, Hard, Adaptive)
- Presentation layer (Godot UI integration)
- Networking layer (host-client multiplayer)
- Data persistence (statistics, settings)

### 1.3 Architectural Goals

| Goal | Description | Rationale |
|------|-------------|-----------|
| **Testability** | Core logic testable without Godot | Enables fast unit tests, CI/CD integration |
| **Separation** | Clear boundaries between layers | Reduces coupling, improves maintainability |
| **Immutability** | State changes via new instances | Predictable behaviour, easier debugging |
| **Minimal Code** | No speculative features | Faster development, reduced complexity |

---

## 2. Architectural Principles

### 2.1 Core Has Zero Godot Dependencies

The Core layer contains all game logic and compiles as a standalone .NET library without any Godot references. This enables:

- Unit testing with standard .NET test frameworks (xUnit, NUnit)
- Faster test execution (no Godot runtime required)
- Clear separation of concerns
- Potential reuse in non-Godot contexts

### 2.2 Functional Paradigm

The architecture favours functional programming principles:

- **Immutable state** using C# records with `with` expressions
- **Pure functions** that produce no side effects
- **Composition** over inheritance
- **Pattern matching** for control flow (switch expressions)

### 2.3 STD-0002 Compliance

All code follows the STD-0002 C# Code Style Rubric:

- Egyptian (cuddled) braces
- Records for value objects and DTOs
- Strongly-typed identifiers (e.g., `PlayerId` not `int`)
- Result pattern for expected failures
- Godot signals for node communication
- No XML documentation comments

---

## 3. System Overview

### 3.1 Layer Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    Presentation Layer                        │
│              (Godot Nodes, Scenes, UI Controls)              │
├─────────────────────────────────────────────────────────────┤
│                    Networking Layer                          │
│           (ENet Multiplayer, State Synchronisation)          │
├─────────────────────────────────────────────────────────────┤
│                      AI Layer                                │
│          (Easy, Medium, Hard, Adaptive Strategies)           │
├─────────────────────────────────────────────────────────────┤
│                    Core Domain                               │
│     (Board, Rules, State Machine - Pure C#, No Godot)        │
└─────────────────────────────────────────────────────────────┘
```

### 3.2 Dependency Direction

Dependencies flow downward only:

- Presentation depends on Core, AI, Networking
- Networking depends on Core
- AI depends on Core
- Core depends on nothing (pure .NET)

### 3.3 Communication Patterns

| Pattern | Usage |
|---------|-------|
| **Events** | Core → Presentation (state changes, game events) |
| **Signals** | Godot node → Godot node (UI interactions) |
| **Direct calls** | Presentation → Core (commands, queries) |
| **RPC** | Host ↔ Client (networked gameplay) |

---

## 4. Core Domain Layer

### 4.1 Overview

The Core layer contains all game logic as pure C# with no external dependencies. It is the foundation upon which all other layers build.

**Location:** `oxs/src/oxs/Core/`

### 4.2 Type Definitions

#### 4.2.1 PlayerId

Strongly-typed player identifier preventing misuse of raw integers.

```csharp
public readonly record struct PlayerId(int Value) {
    public static readonly PlayerId X = new(0);
    public static readonly PlayerId O = new(1);
    public PlayerId Opponent => Value == 0 ? O : X;
}
```

**Design Rationale:** Using a record struct ensures value semantics while preventing accidental use of arbitrary integers as player IDs.

#### 4.2.2 CellState

Represents the state of a single board cell.

```csharp
public enum CellState { Empty, X, O }
```

#### 4.2.3 Board

Immutable representation of the game board.

```csharp
public sealed record Board {
    public int Size { get; }
    public ImmutableArray<CellState> Cells { get; }

    public Board(int size) {
        Size = size;
        Cells = Enumerable.Repeat(CellState.Empty, size * size).ToImmutableArray();
    }

    private Board(int size, ImmutableArray<CellState> cells) {
        Size = size;
        Cells = cells;
    }

    public CellState this[int row, int col] => Cells[row * Size + col];

    public Board WithMove(int row, int col, PlayerId player) {
        var index = row * Size + col;
        var newCells = Cells.SetItem(index, player == PlayerId.X ? CellState.X : CellState.O);
        return new Board(Size, newCells);
    }

    public bool IsCellEmpty(int row, int col) => this[row, col] == CellState.Empty;
    public bool IsFull => !Cells.Contains(CellState.Empty);
}
```

**Design Rationale:**

- Immutable via `ImmutableArray<T>` ensures thread safety and predictable behaviour
- `WithMove` returns a new board instance, preserving the original
- Supports variable board sizes (3x3, 4x4, 5x5) per CORE-001

#### 4.2.4 Move

Value object representing a board position.

```csharp
public readonly record struct Move(int Row, int Col) {
    public int ToIndex(int boardSize) => Row * boardSize + Col;
    public static Move FromIndex(int index, int boardSize) => new(index / boardSize, index % boardSize);
}
```

#### 4.2.5 GameResult

Discriminated union representing game outcomes.

```csharp
public abstract record GameResult {
    public sealed record Win(PlayerId Winner, ImmutableArray<Move> WinningLine) : GameResult;
    public sealed record Draw : GameResult;
    public sealed record InProgress : GameResult;
}
```

**Design Rationale:** Using sealed records as union cases enables exhaustive pattern matching and prevents invalid states.

#### 4.2.6 Result&lt;T&gt;

Result pattern for operations that can fail.

```csharp
public abstract record Result<T> {
    public sealed record Success(T Value) : Result<T>;
    public sealed record Failure(string Error) : Result<T>;

    public TOut Match<TOut>(Func<T, TOut> onSuccess, Func<string, TOut> onFailure) => this switch {
        Success s => onSuccess(s.Value),
        Failure f => onFailure(f.Error),
        _ => throw new InvalidOperationException()
    };
}
```

**Design Rationale:** Explicit error handling without exceptions for expected failures (e.g., invalid moves).

### 4.3 Game Rules

Static class containing pure functions for game logic.

**File:** `Core/GameRules.cs`

```csharp
public static class GameRules {
    public static GameResult CheckResult(Board board, int winLength) {
        var winLine = FindWinningLine(board, winLength);
        return winLine switch {
            not null => new GameResult.Win(
                board[winLine[0].Row, winLine[0].Col] == CellState.X ? PlayerId.X : PlayerId.O,
                winLine.Value),
            null when board.IsFull => new GameResult.Draw(),
            _ => new GameResult.InProgress()
        };
    }

    public static ImmutableArray<Move>? FindWinningLine(Board board, int winLength) {
        var lines = GetAllLines(board.Size, winLength);
        foreach (var line in lines) {
            var first = board[line[0].Row, line[0].Col];
            if (first == CellState.Empty) continue;
            if (line.All(m => board[m.Row, m.Col] == first)) {
                return line;
            }
        }
        return null;
    }

    public static IEnumerable<ImmutableArray<Move>> GetAllLines(int boardSize, int winLength) {
        // Horizontal lines
        for (int row = 0; row < boardSize; row++) {
            for (int startCol = 0; startCol <= boardSize - winLength; startCol++) {
                yield return Enumerable.Range(0, winLength)
                    .Select(i => new Move(row, startCol + i))
                    .ToImmutableArray();
            }
        }

        // Vertical lines
        for (int col = 0; col < boardSize; col++) {
            for (int startRow = 0; startRow <= boardSize - winLength; startRow++) {
                yield return Enumerable.Range(0, winLength)
                    .Select(i => new Move(startRow + i, col))
                    .ToImmutableArray();
            }
        }

        // Diagonal lines (top-left to bottom-right)
        for (int row = 0; row <= boardSize - winLength; row++) {
            for (int col = 0; col <= boardSize - winLength; col++) {
                yield return Enumerable.Range(0, winLength)
                    .Select(i => new Move(row + i, col + i))
                    .ToImmutableArray();
            }
        }

        // Diagonal lines (top-right to bottom-left)
        for (int row = 0; row <= boardSize - winLength; row++) {
            for (int col = winLength - 1; col < boardSize; col++) {
                yield return Enumerable.Range(0, winLength)
                    .Select(i => new Move(row + i, col - i))
                    .ToImmutableArray();
            }
        }
    }

    public static IEnumerable<Move> GetValidMoves(Board board) {
        for (int row = 0; row < board.Size; row++) {
            for (int col = 0; col < board.Size; col++) {
                if (board.IsCellEmpty(row, col)) {
                    yield return new Move(row, col);
                }
            }
        }
    }
}
```

**Requirements Coverage:**

- CORE-002: Configurable win conditions via `winLength` parameter
- CORE-003: Win detection within 500ms (pure computation, well under target)
- CORE-004: Draw detection when board is full

### 4.4 Game State Machine

Manages game state transitions and events.

**File:** `Core/GameStateMachine.cs`

```csharp
public enum GamePhase { Setup, Playing, GameOver, MatchOver }

public sealed record GameStateData {
    public required Board Board { get; init; }
    public required PlayerId CurrentPlayer { get; init; }
    public required int WinLength { get; init; }
    public required GamePhase Phase { get; init; }
    public GameResult? Result { get; init; }
    public int Player1Score { get; init; }
    public int Player2Score { get; init; }
    public int CurrentRound { get; init; } = 1;
    public int TotalRounds { get; init; } = 1;
}

public sealed class GameStateMachine {
    private GameStateData _state;

    public GameStateData State => _state;

    public event Action<GameStateData>? StateChanged;
    public event Action<Move, PlayerId>? MoveMade;
    public event Action<GameResult>? GameEnded;

    public GameStateMachine(int boardSize, int winLength, int totalRounds = 1) {
        _state = new GameStateData {
            Board = new Board(boardSize),
            CurrentPlayer = PlayerId.X,
            WinLength = winLength,
            Phase = GamePhase.Playing,
            TotalRounds = totalRounds
        };
    }

    public Result<GameStateData> MakeMove(Move move, PlayerId player) {
        if (_state.Phase != GamePhase.Playing) {
            return new Result<GameStateData>.Failure("Game is not in playing phase");
        }
        if (player != _state.CurrentPlayer) {
            return new Result<GameStateData>.Failure("Not this player's turn");
        }
        if (!_state.Board.IsCellEmpty(move.Row, move.Col)) {
            return new Result<GameStateData>.Failure("Cell is not empty");
        }

        var newBoard = _state.Board.WithMove(move.Row, move.Col, player);
        var result = GameRules.CheckResult(newBoard, _state.WinLength);

        var newPhase = result switch {
            GameResult.InProgress => GamePhase.Playing,
            _ => GamePhase.GameOver
        };

        var (p1Score, p2Score) = result switch {
            GameResult.Win w when w.Winner == PlayerId.X => (_state.Player1Score + 1, _state.Player2Score),
            GameResult.Win w when w.Winner == PlayerId.O => (_state.Player1Score, _state.Player2Score + 1),
            GameResult.Draw => (_state.Player1Score, _state.Player2Score),
            _ => (_state.Player1Score, _state.Player2Score)
        };

        _state = _state with {
            Board = newBoard,
            CurrentPlayer = player.Opponent,
            Phase = newPhase,
            Result = result is GameResult.InProgress ? null : result,
            Player1Score = p1Score,
            Player2Score = p2Score
        };

        MoveMade?.Invoke(move, player);
        StateChanged?.Invoke(_state);

        if (result is not GameResult.InProgress) {
            GameEnded?.Invoke(result);
        }

        return new Result<GameStateData>.Success(_state);
    }

    public void StartNextRound(PlayerId startingPlayer) {
        _state = _state with {
            Board = new Board(_state.Board.Size),
            CurrentPlayer = startingPlayer,
            Phase = GamePhase.Playing,
            Result = null,
            CurrentRound = _state.CurrentRound + 1
        };
        StateChanged?.Invoke(_state);
    }
}
```

**Requirements Coverage:**

- CORE-007: Match play with multiple rounds via `TotalRounds`
- CORE-008: Alternating starting player via `StartNextRound`
- LOCAL-005: Loser starts next game (handled by caller)
- LOCAL-006: No undo (no undo method provided)

---

## 5. AI Layer

### 5.1 Overview

The AI layer provides computer opponents at various difficulty levels. All AI implementations are pure C# and depend only on the Core layer.

**Location:** `oxs/src/oxs/Core/AI/`

### 5.2 AI Interface

```csharp
public interface IAIPlayer {
    Move SelectMove(Board board, PlayerId player, int winLength);
}
```

### 5.3 Difficulty Levels

#### 5.3.1 EasyAI

Random move selection with no strategy.

```csharp
public sealed class EasyAI : IAIPlayer {
    private readonly Random _random = new();

    public Move SelectMove(Board board, PlayerId player, int winLength) {
        var validMoves = GameRules.GetValidMoves(board).ToList();
        return validMoves[_random.Next(validMoves.Count)];
    }
}
```

**Requirements Coverage:** AI-002 (wins less than 20% against competent play)

#### 5.3.2 MediumAI

Heuristic-based AI that blocks and takes winning moves.

```csharp
public sealed class MediumAI : IAIPlayer {
    private readonly Random _random = new();

    public Move SelectMove(Board board, PlayerId player, int winLength) {
        // 1. Win if possible
        var winMove = FindWinningMove(board, player, winLength);
        if (winMove.HasValue) return winMove.Value;

        // 2. Block opponent win
        var blockMove = FindWinningMove(board, player.Opponent, winLength);
        if (blockMove.HasValue) return blockMove.Value;

        // 3. Take center if available
        int center = board.Size / 2;
        if (board.IsCellEmpty(center, center)) {
            return new Move(center, center);
        }

        // 4. Random valid move
        var validMoves = GameRules.GetValidMoves(board).ToList();
        return validMoves[_random.Next(validMoves.Count)];
    }

    private static Move? FindWinningMove(Board board, PlayerId player, int winLength) {
        foreach (var move in GameRules.GetValidMoves(board)) {
            var testBoard = board.WithMove(move.Row, move.Col, player);
            var result = GameRules.CheckResult(testBoard, winLength);
            if (result is GameResult.Win) {
                return move;
            }
        }
        return null;
    }
}
```

**Requirements Coverage:** AI-003 (never misses immediate win, always blocks)

#### 5.3.3 HardAI

Minimax algorithm with alpha-beta pruning for optimal play.

```csharp
public sealed class HardAI : IAIPlayer {
    private const int MaxDepth = 9;

    public Move SelectMove(Board board, PlayerId player, int winLength) {
        var (_, bestMove) = Minimax(board, player, player, winLength, MaxDepth, int.MinValue, int.MaxValue);
        return bestMove ?? GameRules.GetValidMoves(board).First();
    }

    private (int Score, Move? BestMove) Minimax(
        Board board, PlayerId currentPlayer, PlayerId maximisingPlayer,
        int winLength, int depth, int alpha, int beta) {

        var result = GameRules.CheckResult(board, winLength);

        if (result is GameResult.Win win) {
            return (win.Winner == maximisingPlayer ? 100 + depth : -100 - depth, null);
        }
        if (result is GameResult.Draw || depth == 0) {
            return (0, null);
        }

        var validMoves = GameRules.GetValidMoves(board).ToList();
        Move? bestMove = null;

        if (currentPlayer == maximisingPlayer) {
            int maxScore = int.MinValue;
            foreach (var move in validMoves) {
                var newBoard = board.WithMove(move.Row, move.Col, currentPlayer);
                var (score, _) = Minimax(newBoard, currentPlayer.Opponent, maximisingPlayer, winLength, depth - 1, alpha, beta);
                if (score > maxScore) {
                    maxScore = score;
                    bestMove = move;
                }
                alpha = Math.Max(alpha, score);
                if (beta <= alpha) break;
            }
            return (maxScore, bestMove);
        } else {
            int minScore = int.MaxValue;
            foreach (var move in validMoves) {
                var newBoard = board.WithMove(move.Row, move.Col, currentPlayer);
                var (score, _) = Minimax(newBoard, currentPlayer.Opponent, maximisingPlayer, winLength, depth - 1, alpha, beta);
                if (score < minScore) {
                    minScore = score;
                    bestMove = move;
                }
                beta = Math.Min(beta, score);
                if (beta <= alpha) break;
            }
            return (minScore, bestMove);
        }
    }
}
```

**Requirements Coverage:** AI-004 (near-optimal play, beatable 5-15% of the time)

**Design Notes:**

- Depth limit of 9 ensures reasonable computation time on larger boards
- Alpha-beta pruning significantly reduces search space
- Depth-aware scoring prefers faster wins

#### 5.3.4 AdaptiveAI

Adjusts difficulty based on player performance.

```csharp
public sealed class AdaptiveAI : IAIPlayer {
    private readonly EasyAI _easy = new();
    private readonly MediumAI _medium = new();
    private readonly HardAI _hard = new();

    private int _playerWinStreak;
    private int _aiWinStreak;
    private AIDifficulty _currentDifficulty = AIDifficulty.Medium;

    public Move SelectMove(Board board, PlayerId player, int winLength) {
        return _currentDifficulty switch {
            AIDifficulty.Easy => _easy.SelectMove(board, player, winLength),
            AIDifficulty.Medium => _medium.SelectMove(board, player, winLength),
            AIDifficulty.Hard => _hard.SelectMove(board, player, winLength),
            _ => _medium.SelectMove(board, player, winLength)
        };
    }

    public void RecordGameResult(bool aiWon) {
        if (aiWon) {
            _aiWinStreak++;
            _playerWinStreak = 0;
        } else {
            _playerWinStreak++;
            _aiWinStreak = 0;
        }

        // Adjust after 3 consecutive results
        if (_playerWinStreak >= 3 && _currentDifficulty != AIDifficulty.Hard) {
            _currentDifficulty = (AIDifficulty)((int)_currentDifficulty + 1);
            _playerWinStreak = 0;
        } else if (_aiWinStreak >= 3 && _currentDifficulty != AIDifficulty.Easy) {
            _currentDifficulty = (AIDifficulty)((int)_currentDifficulty - 1);
            _aiWinStreak = 0;
        }
    }
}
```

**Requirements Coverage:** AI-005 (adjusts after 3+ consecutive wins/losses)

### 5.4 AI Factory

```csharp
public enum AIDifficulty { Easy, Medium, Hard }

public static class AIFactory {
    public static IAIPlayer Create(AIDifficulty difficulty) => difficulty switch {
        AIDifficulty.Easy => new EasyAI(),
        AIDifficulty.Medium => new MediumAI(),
        AIDifficulty.Hard => new HardAI(),
        _ => new MediumAI()
    };
}
```

---

## 6. Presentation Layer

### 6.1 Overview

The Presentation layer contains all Godot-specific code including scenes, nodes, and UI controls. It consumes the Core layer via events and direct method calls.

**Location:** `oxs/src/oxs/Presentation/`

### 6.2 Views

#### 6.2.1 CellView

Individual cell in the game grid.

```csharp
public partial class CellView : Control {
    [Signal] public delegate void ClickedEventHandler();

    private CellState _state = CellState.Empty;
    private bool _interactive = true;

    public void SetState(CellState state) {
        _state = state;
        QueueRedraw();
    }

    public void SetInteractive(bool interactive) {
        _interactive = interactive;
    }

    public void HighlightAsWinner() {
        // Trigger win animation
    }

    public override void _GuiInput(InputEvent @event) {
        if (!_interactive) return;
        if (@event is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left }) {
            EmitSignal(SignalName.Clicked);
        }
    }

    public override void _Draw() {
        // Draw X, O, or empty based on _state
    }
}
```

#### 6.2.2 BoardView

Container for the game grid.

```csharp
public partial class BoardView : Control {
    [Signal] public delegate void CellClickedEventHandler(int row, int col);

    private int _boardSize;
    private readonly List<CellView> _cells = new();

    public void Initialise(int boardSize) {
        _boardSize = boardSize;
        CreateCells();
    }

    public void UpdateCell(int row, int col, CellState state) {
        var index = row * _boardSize + col;
        _cells[index].SetState(state);
    }

    public void HighlightWinningLine(ImmutableArray<Move> line) {
        foreach (var move in line) {
            var index = move.Row * _boardSize + move.Col;
            _cells[index].HighlightAsWinner();
        }
    }

    public void SetInteractive(bool interactive) {
        foreach (var cell in _cells) {
            cell.SetInteractive(interactive);
        }
    }

    private void CreateCells() {
        foreach (var cell in _cells) {
            cell.QueueFree();
        }
        _cells.Clear();

        for (int row = 0; row < _boardSize; row++) {
            for (int col = 0; col < _boardSize; col++) {
                var cell = CreateCell(row, col);
                _cells.Add(cell);
                AddChild(cell);
            }
        }
    }

    private CellView CreateCell(int row, int col) {
        var cell = new CellView();
        cell.Clicked += () => EmitSignal(SignalName.CellClicked, row, col);
        return cell;
    }
}
```

**Requirements Coverage:**

- CORE-001: Variable board sizes (created dynamically)
- LOCAL-001: Mouse/touch input via signals
- UI-002: Animated visual style (via cell animations)

### 6.3 Controllers

#### 6.3.1 GameController

Main orchestrator connecting Core logic to UI.

```csharp
public partial class GameController : Node {
    [Export] public BoardView BoardView { get; set; } = null!;
    [Export] public Label StatusLabel { get; set; } = null!;
    [Export] public Label ScoreLabel { get; set; } = null!;

    private GameStateMachine _game = null!;
    private IAIPlayer? _aiPlayer;
    private PlayerId _humanPlayer = PlayerId.X;

    public override void _Ready() {
        BoardView.CellClicked += OnCellClicked;
    }

    public void StartGame(GameConfig config) {
        _game = new GameStateMachine(config.BoardSize, config.WinLength, config.TotalRounds);
        _game.StateChanged += OnStateChanged;
        _game.MoveMade += OnMoveMade;
        _game.GameEnded += OnGameEnded;

        if (config.GameMode == GameMode.VsAI) {
            _aiPlayer = AIFactory.Create(config.AIDifficulty);
        }

        BoardView.Initialise(config.BoardSize);
        UpdateUI();
    }

    private void OnCellClicked(int row, int col) {
        if (_game.State.CurrentPlayer != _humanPlayer) return;

        var result = _game.MakeMove(new Move(row, col), _humanPlayer);
        result.Match(
            onSuccess: _ => { },
            onFailure: error => GD.PrintErr($"Invalid move: {error}")
        );
    }

    private void OnStateChanged(GameStateData state) {
        UpdateUI();

        if (_aiPlayer != null && state.Phase == GamePhase.Playing && state.CurrentPlayer != _humanPlayer) {
            ScheduleAIMove();
        }
    }

    private void OnMoveMade(Move move, PlayerId player) {
        var state = player == PlayerId.X ? CellState.X : CellState.O;
        BoardView.UpdateCell(move.Row, move.Col, state);
    }

    private void OnGameEnded(GameResult result) {
        BoardView.SetInteractive(false);

        if (result is GameResult.Win win) {
            BoardView.HighlightWinningLine(win.WinningLine);
            StatusLabel.Text = $"Player {(win.Winner == PlayerId.X ? "X" : "O")} wins!";
        } else {
            StatusLabel.Text = "Draw!";
        }
    }

    private async void ScheduleAIMove() {
        BoardView.SetInteractive(false);
        StatusLabel.Text = "AI thinking...";

        await ToSignal(GetTree().CreateTimer(0.7), SceneTreeTimer.SignalName.Timeout);

        var move = _aiPlayer!.SelectMove(_game.State.Board, _game.State.CurrentPlayer, _game.State.WinLength);
        _game.MakeMove(move, _game.State.CurrentPlayer);

        BoardView.SetInteractive(true);
    }

    private void UpdateUI() {
        var state = _game.State;
        StatusLabel.Text = state.Phase == GamePhase.Playing
            ? $"Player {(state.CurrentPlayer == PlayerId.X ? "X" : "O")}'s turn"
            : "";
        ScoreLabel.Text = $"X: {state.Player1Score} | O: {state.Player2Score}";
    }
}
```

**Requirements Coverage:**

- AI-007: 0.5-1.0 second thinking delay (0.7s timer)
- AI-008: Difficulty locked during gameplay (no change method)

### 6.4 Configuration

```csharp
public record GameConfig {
    public required int BoardSize { get; init; }
    public required int WinLength { get; init; }
    public required GameMode GameMode { get; init; }
    public AIDifficulty AIDifficulty { get; init; } = AIDifficulty.Medium;
    public int TotalRounds { get; init; } = 1;
}

public enum GameMode { Local, VsAI, Network }
```

---

## 7. Networking Layer

### 7.1 Overview

The Networking layer enables two-player games over a network using Godot's built-in ENet multiplayer API. It follows a host-client architecture where the host is authoritative.

**Location:** `oxs/src/oxs/Networking/`

### 7.2 Network Messages

```csharp
public abstract record NetworkMessage {
    public sealed record MoveRequest(Move Move) : NetworkMessage;
    public sealed record StateSync(GameStateData State) : NetworkMessage;
    public sealed record GameStart(GameConfig Config) : NetworkMessage;
    public sealed record PlayerDisconnected(PlayerId Player) : NetworkMessage;
    public sealed record Reconnected(PlayerId Player) : NetworkMessage;
}
```

### 7.3 NetworkManager

```csharp
public partial class NetworkManager : Node {
    [Signal] public delegate void PeerConnectedEventHandler(long peerId);
    [Signal] public delegate void PeerDisconnectedEventHandler(long peerId);
    [Signal] public delegate void MoveReceivedEventHandler(int row, int col);
    [Signal] public delegate void StateSyncReceivedEventHandler(byte[] stateData);

    private ENetMultiplayerPeer _peer = new();

    public bool IsHost => Multiplayer.IsServer();
    public bool IsConnected => _peer.GetConnectionStatus() == MultiplayerPeer.ConnectionStatus.Connected;

    public Result<bool> HostGame(int port) {
        var error = _peer.CreateServer(port, 1);
        if (error != Error.Ok) {
            return new Result<bool>.Failure($"Failed to create server: {error}");
        }
        Multiplayer.MultiplayerPeer = _peer;
        return new Result<bool>.Success(true);
    }

    public Result<bool> JoinGame(string address, int port) {
        var error = _peer.CreateClient(address, port);
        if (error != Error.Ok) {
            return new Result<bool>.Failure($"Failed to connect: {error}");
        }
        Multiplayer.MultiplayerPeer = _peer;
        return new Result<bool>.Success(true);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false)]
    public void SendMove(int row, int col) {
        EmitSignal(SignalName.MoveReceived, row, col);
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false)]
    public void SyncState(byte[] stateData) {
        EmitSignal(SignalName.StateSyncReceived, stateData);
    }

    public override void _Ready() {
        Multiplayer.PeerConnected += OnPeerConnected;
        Multiplayer.PeerDisconnected += OnPeerDisconnected;
    }

    private void OnPeerConnected(long id) {
        EmitSignal(SignalName.PeerConnected, id);
    }

    private void OnPeerDisconnected(long id) {
        EmitSignal(SignalName.PeerDisconnected, id);
    }
}
```

**Requirements Coverage:**

- NET-001: Direct IP connection
- NET-002: Host-client architecture (host is server)
- NET-004/005: Disconnection handling via signals
- NET-008: Max 1 client (passed to CreateServer)
- CON-004: Uses Godot's built-in multiplayer API

### 7.4 Synchronisation

The host maintains the authoritative `GameStateMachine`. On each state change:

1. Host serialises `GameStateData` to bytes
2. Host calls `RpcId(clientId, "SyncState", stateBytes)`
3. Client deserialises and updates local view

Client moves are sent to host for validation:

1. Client calls `RpcId(1, "SendMove", row, col)`
2. Host validates and applies move via `GameStateMachine`
3. Host broadcasts new state to client

---

## 8. Persistence Layer

### 8.1 Statistics Manager

```csharp
public sealed class StatisticsManager {
    private const string SavePath = "user://statistics.json";

    public PlayerStatistics Statistics { get; private set; } = new();

    public void RecordWin() {
        Statistics = Statistics with { Wins = Statistics.Wins + 1 };
        Save();
    }

    public void RecordLoss() {
        Statistics = Statistics with { Losses = Statistics.Losses + 1 };
        Save();
    }

    public void RecordDraw() {
        Statistics = Statistics with { Draws = Statistics.Draws + 1 };
        Save();
    }

    public void Load() {
        if (!FileAccess.FileExists(SavePath)) return;
        using var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Read);
        var json = file.GetAsText();
        Statistics = JsonSerializer.Deserialize<PlayerStatistics>(json) ?? new();
    }

    private void Save() {
        using var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Write);
        var json = JsonSerializer.Serialize(Statistics);
        file.StoreString(json);
    }
}

public sealed record PlayerStatistics {
    public int Wins { get; init; }
    public int Losses { get; init; }
    public int Draws { get; init; }
}
```

**Requirements Coverage:** CORE-006 (persistent statistics)

---

## 9. Project Structure

```
oxs/src/oxs/
├── Core/                          # Pure C# - NO Godot dependencies
│   ├── Types.cs                   # PlayerId, CellState, Board, Move, GameResult, Result<T>
│   ├── GameRules.cs               # Win detection, valid moves, line generation
│   ├── GameStateMachine.cs        # Game state management and transitions
│   └── AI/
│       ├── IAIPlayer.cs           # AI interface
│       ├── EasyAI.cs              # Random move selection
│       ├── MediumAI.cs            # Heuristic-based AI
│       ├── HardAI.cs              # Minimax with alpha-beta pruning
│       ├── AdaptiveAI.cs          # Difficulty adjustment
│       └── AIFactory.cs           # AI instantiation
│
├── Presentation/                   # Godot-specific UI
│   ├── Views/
│   │   ├── BoardView.cs           # Grid display and cell management
│   │   ├── CellView.cs            # Individual cell rendering
│   │   └── WinLineView.cs         # Winning line animation
│   ├── Controllers/
│   │   ├── GameController.cs      # Main game orchestration
│   │   ├── MenuController.cs      # Menu navigation
│   │   └── SettingsController.cs  # Configuration management
│   ├── Screens/
│   │   ├── MainMenuScreen.tscn    # Main menu scene
│   │   ├── GameScreen.tscn        # Gameplay scene
│   │   └── ResultScreen.tscn      # Game over screen
│   └── Config/
│       └── GameConfig.cs          # Game configuration record
│
├── Networking/                     # Multiplayer support
│   ├── NetworkManager.cs          # Connection management
│   ├── GameSynchroniser.cs        # State synchronisation
│   └── Messages.cs                # Network message types
│
├── Persistence/                    # Data storage
│   ├── StatisticsManager.cs       # Win/loss/draw tracking
│   └── SettingsManager.cs         # User preferences
│
├── OXS.csproj                      # Main project file
└── project.godot                   # Godot project file

oxs/tests/OXS.Tests/                # Unit tests (pure C#)
├── OXS.Tests.csproj
├── Core/
│   ├── BoardTests.cs
│   ├── GameRulesTests.cs
│   └── GameStateMachineTests.cs
└── AI/
    ├── EasyAITests.cs
    ├── MediumAITests.cs
    └── HardAITests.cs
```

---

## 10. Key Design Decisions

### 10.1 Immutable State

**Decision:** All game state uses immutable records with `with` expressions.

**Rationale:**

- Prevents accidental mutation bugs
- Enables safe state sharing across threads
- Simplifies debugging (states can be logged/compared)
- Aligns with STD-0002 functional paradigm

### 10.2 Pure Core Layer

**Decision:** Core layer has zero Godot dependencies.

**Rationale:**

- Unit tests run without Godot runtime (~10x faster)
- Enables CI/CD with standard .NET tooling
- Clear architectural boundary
- Potential reuse in non-Godot contexts

### 10.3 Signal-Based Communication

**Decision:** Use Godot signals for all node-to-node communication.

**Rationale:**

- Decouples nodes from each other
- Follows STD-0002 guidelines
- Enables easy scene composition
- Supports editor-based connection

### 10.4 Result Pattern for Failures

**Decision:** Use `Result<T>` instead of exceptions for expected failures.

**Rationale:**

- Makes error handling explicit at call sites
- Prevents forgotten try-catch blocks
- Enables functional composition
- Aligns with STD-0002 guidelines

### 10.5 Host-Authoritative Networking

**Decision:** Host runs game logic; clients send inputs and receive state.

**Rationale:**

- Prevents cheating (host validates all moves)
- Simplifies synchronisation
- Aligns with NET-002 requirement
- Standard pattern for turn-based games

### 10.6 Factory Pattern for AI

**Decision:** Use `AIFactory.Create()` for AI instantiation.

**Rationale:**

- Decouples callers from concrete AI implementations
- Enables easy addition of new difficulties
- Supports runtime difficulty selection
- Aligns with AI-008 (difficulty locked during game)

---

## 11. Requirements Traceability

| Requirement | Architecture Component | Section |
|-------------|------------------------|---------|
| CORE-001 | Board.Size, BoardView.Initialise | 4.2.3, 6.2.2 |
| CORE-002 | GameRules.CheckResult, winLength | 4.3 |
| CORE-003 | GameRules.CheckResult | 4.3 |
| CORE-004 | GameRules.CheckResult | 4.3 |
| CORE-006 | StatisticsManager | 8.1 |
| CORE-007 | GameStateData.TotalRounds | 4.4 |
| CORE-008 | GameStateMachine.StartNextRound | 4.4 |
| LOCAL-001 | CellView, BoardView signals | 6.2 |
| LOCAL-005 | GameStateMachine.StartNextRound | 4.4 |
| LOCAL-006 | No undo method in GameStateMachine | 4.4 |
| AI-001 | EasyAI, MediumAI, HardAI | 5.3 |
| AI-002 | EasyAI | 5.3.1 |
| AI-003 | MediumAI | 5.3.2 |
| AI-004 | HardAI | 5.3.3 |
| AI-005 | AdaptiveAI | 5.3.4 |
| AI-007 | GameController.ScheduleAIMove | 6.3.1 |
| AI-008 | AIFactory, no runtime change | 5.4 |
| NET-001 | NetworkManager.HostGame/JoinGame | 7.3 |
| NET-002 | Host-client architecture | 7.1 |
| NET-008 | CreateServer maxClients=1 | 7.3 |
| CON-004 | ENetMultiplayerPeer | 7.3 |

---

## Appendices

### A. Glossary

| Term | Definition |
|------|------------|
| Core Layer | Pure C# game logic with no Godot dependencies |
| Presentation Layer | Godot-specific UI code |
| Host | Authoritative game instance in networked play |
| Client | Connecting player instance in networked play |
| Result Pattern | Error handling via `Result<T>` instead of exceptions |

### B. References

| Document | Description |
|----------|-------------|
| OXS-REQUIREMENTS.md | Software requirements specification |
| OXS-GDD.md | Game design document |
| STD-0002 | C# code style rubric |
| Godot 4.5 Docs | Official Godot documentation |

### C. Revision History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2026-01-16 | Software Architect | Initial architecture specification |
