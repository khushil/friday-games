# C# Code Style and Architecture Rubric for Godot 4.5

## Reference Guide for Friday Games

---

## Philosophy

> We build **SYSTEMS** from which behaviour **EMERGES**, not scripts that tell the computer what to do.

Code should be elegant, minimal, and self-documenting. Less code is ALWAYS better than more code for the same capability.

The codebase reads like a top-down narrative where each function is a recursive tree of actions - individually understandable and composable, each solving its own clearly defined problem.

Features should be designed so future improvements slot in simply, and existing functionality comes "for free".

---

## Core Principles

### 1. MINIMAL CODE - ALWAYS

- Write the absolute minimum code to accomplish the task
- Combine elements into shared logic using advanced techniques
- Consolidate or UPDATE existing components rather than adding new ones
- No boilerplate - structure code to prevent it entirely
- No "fallback mechanisms" - they hide real errors
- If you can delete code without losing capability, do it

### 2. STRONGLY-TYPED EVERYTHING

- NEVER use magic strings - use enums, constants, `nameof()`, or reflection
- Strongly-typed IDs over primitive obsession (`PlayerId` not `int`)
- Generics create type-safe abstractions
- Exception: Resource paths may use string literals (`"res://..."`)

### 3. FUNCTIONAL PARADIGM PREFERRED

- Functional composition with `Func<T>`, `Action<T>` over OO inheritance
- Prefer immutability (records, `init`, `readonly`)
- Pure functions - same input always produces same output
- Higher-order functions for cross-cutting concerns
- Embrace recursion where it makes sense - it's elegant
- Favour imperative clarity over declarative magic

### 4. MODERN C# FEATURES (C# 12 / .NET 8)

- Pattern matching: `is`, `switch`, property patterns, list patterns
- Switch expressions over switch statements
- Records for DTOs and value objects
- Primary constructors for services
- Collection expressions `[1, 2, 3]`
- Required members
- File-scoped namespaces
- Global usings
- Raw string literals `"""multi-line"""`
- Generic math where applicable
- Discards `_` for unused values
- Named tuples for ad-hoc structures

### 5. SEPARATION OF CONCERNS

- Distinct layers communicating through well-defined interfaces
- Game logic as the core (rich, not anaemic entities)
- Core game logic has ZERO Godot dependencies where possible
- Presentation (Nodes) depends on Core, never reverse

### 6. SMALL COMPONENTS

- Many small functions over few large ones
- Many small files over few large ones
- Each component solves ONE clearly defined problem
- Small type-safe functions compose elegantly
- Small files, classes, namespaces create elegant structure

### 7. GODOT CONCURRENCY DONE RIGHT

- Understand `_Process(double delta)` vs `_PhysicsProcess(double delta)`
- Use signals for decoupled communication between nodes
- `CallDeferred()` for thread-safe node tree manipulation
- `ToSignal()` for async operations with Godot signals
- Never block the main thread - use async/await patterns

---

## Formatting Rules

### Egyptian Braces - MANDATORY

Cuddled braces for ALL code blocks:

```csharp
// ✅ CORRECT - Egyptian/cuddled braces
if (condition) {
    DoSomething();
} else {
    DoSomethingElse();
}

public partial class Player : CharacterBody3D {
    public PlayerId Id { get; }
}

// ❌ WRONG - Allman style (never use this)
if (condition)
{
    DoSomething();
}
```

### No Code Cramming

Never multiple statements on one line:

```csharp
// ✅ CORRECT
if (health <= 0) {
    Die();
}

// ❌ WRONG
if (health <= 0) { Die(); }
```

### Early Returns + Local Functions

Avoid deep nesting:

```csharp
// ✅ CORRECT - Flat with early returns and local functions
public Result TakeDamage(DamageInfo damage) {
    if (IsDead) {
        return Result.Fail("Already dead");
    }

    if (IsInvulnerable) {
        return Result.Ok();
    }

    return ApplyDamage(damage);

    Result ApplyDamage(DamageInfo dmg) {
        var finalDamage = CalculateFinalDamage(dmg);
        Health -= finalDamage;
        EmitSignal(SignalName.DamageTaken, finalDamage);

        if (Health <= 0) {
            Die();
        }

        return Result.Ok();
    }
}
```

### No XML Documentation Comments

Code speaks for itself through naming. Only add comments when logic is inherently unclear or abstract.

```csharp
// ✅ CORRECT - Self-documenting name, no comment needed
public bool CanAttack => !IsDead && !IsStunned && _attackCooldown <= 0;

// ❌ WRONG - Expensive, redundant XML docs
/// <summary>
/// Determines whether the character can currently attack.
/// </summary>
public bool CanAttack => !IsDead && !IsStunned && _attackCooldown <= 0;
```

---

## Patterns to Use

### Pattern Matching - Use Extensively

```csharp
// Switch expressions for mapping
public string GetStateName(CharacterState state) => state switch {
    CharacterState.Idle => "Standing",
    CharacterState.Moving => "Running",
    CharacterState.Attacking => "Combat",
    CharacterState.Dead => "Deceased",
    _ => throw new ArgumentOutOfRangeException(nameof(state))
};

// Property patterns for complex conditions
public float CalculateDamage(DamageInfo damage) => damage switch {
    { Type: DamageType.Physical, IsCritical: true } => damage.Amount * 2f,
    { Type: DamageType.Physical } => damage.Amount,
    { Type: DamageType.Fire, Source.HasBuff: BuffType.FireMastery } => damage.Amount * 1.5f,
    { Type: DamageType.Fire } => damage.Amount,
    _ => damage.Amount
};

// List patterns for combo detection
public bool IsValidCombo(IReadOnlyList<InputAction> inputs) => inputs switch {
    [InputAction.Light, InputAction.Light, InputAction.Heavy] => true,
    [InputAction.Heavy, InputAction.Special] => true,
    [InputAction.Light, .., InputAction.Special] => true,
    [] => false,
    _ => false
};

// Combining patterns
public Result ValidateSpawn(SpawnRequest request) => request switch {
    null => Result.Fail("Request is null"),
    { Position: var p } when !IsValidPosition(p) => Result.Fail("Invalid position"),
    { EntityType: null or { Length: 0 } } => Result.Fail("No entity type"),
    _ => Result.Ok()
};
```

### Records for Value Objects and DTOs

```csharp
// Value object with validation factory
public sealed record Health {
    public int Current { get; private init; }
    public int Max { get; }

    private Health(int current, int max) => (Current, Max) = (current, max);

    public static Result<Health> Create(int max) => max switch {
        <= 0 => Result.Failure<Health>("Max health must be positive"),
        _ => Result.Success(new Health(max, max))
    };

    public Health TakeDamage(int amount) => this with {
        Current = Math.Max(0, Current - amount)
    };

    public bool IsDead => Current <= 0;
}

// DTO as positional record
public sealed record DamageInfo(
    int Amount,
    DamageType Type,
    EntityId SourceId,
    bool IsCritical = false);

public sealed record SpawnRequest(
    string EntityType,
    Vector3 Position,
    float Rotation = 0f);
```

### Primary Constructors for Autoload Services

```csharp
// Note: Godot autoloads typically use _Ready, but services can use primary constructors
public sealed class CombatService(
    IEntityRegistry registry,
    IDamageCalculator calculator) {

    public Result ApplyDamage(
        EntityId targetId,
        DamageInfo damage) {

        var target = registry.Get(targetId);
        if (target is null) {
            GD.PushWarning($"Entity {targetId} not found");
            return Result.NotFound();
        }

        var finalDamage = calculator.Calculate(damage, target);
        var result = target.TakeDamage(finalDamage);

        return result;
    }
}
```

### Strongly-Typed IDs

```csharp
public readonly record struct PlayerId(int Value) : IEquatable<PlayerId> {
    public static PlayerId New() => new(NextId());
    public static PlayerId Empty => new(0);

    private static int _nextId = 1;
    private static int NextId() => Interlocked.Increment(ref _nextId);

    public override string ToString() => $"Player:{Value}";
}

public readonly record struct EntityId(ulong Value) : IEquatable<EntityId> {
    public static EntityId New() => new(GD.Randi() | ((ulong)GD.Randi() << 32));
    public static EntityId Empty => new(0);
    public override string ToString() => $"Entity:{Value:X16}";
}

public readonly record struct WeaponId(int Value) : IEquatable<WeaponId> {
    public static WeaponId FromResource(string path) => new(path.GetHashCode());
    public override string ToString() => $"Weapon:{Value}";
}
```

### Result Pattern

No exceptions for expected failures:

```csharp
public abstract record Result {
    public bool IsSuccess => this is Success;
    public bool IsFailure => !IsSuccess;

    public sealed record Success : Result;
    public sealed record Failure(string Error) : Result;
    public sealed record NotFound : Result;
    public sealed record ValidationFailed(IReadOnlyList<string> Errors) : Result;

    public static Result Ok() => new Success();
    public static Result Fail(string error) => new Failure(error);
    public static Result NotFound() => new NotFound();
}

public abstract record Result<T> {
    public bool IsSuccess => this is Success;
    public bool IsFailure => !IsSuccess;

    public sealed record Success(T Value) : Result<T>;
    public sealed record Failure(string Error) : Result<T>;
    public sealed record NotFound : Result<T>;

    public static Result<T> Ok(T value) => new Success(value);
    public static Result<T> Fail(string error) => new Failure(error);

    public TResult Match<TResult>(
        Func<T, TResult> onSuccess,
        Func<string, TResult> onFailure) => this switch {
            Success s => onSuccess(s.Value),
            Failure f => onFailure(f.Error),
            NotFound => onFailure("Not found"),
            _ => throw new InvalidOperationException()
        };
}
```

### Functional Composition

```csharp
// Damage modifier pipeline
public static Func<DamageInfo, DamageInfo> WithArmorReduction(float armor) {
    return damage => damage with {
        Amount = (int)(damage.Amount * (1f - armor / 100f))
    };
}

public static Func<DamageInfo, DamageInfo> WithCriticalMultiplier(float multiplier) {
    return damage => damage.IsCritical
        ? damage with { Amount = (int)(damage.Amount * multiplier) }
        : damage;
}

public static Func<DamageInfo, DamageInfo> WithElementalResistance(
    DamageType type,
    float resistance) {
    return damage => damage.Type == type
        ? damage with { Amount = (int)(damage.Amount * (1f - resistance)) }
        : damage;
}

// Usage: compose modifiers
public DamageInfo ApplyModifiers(DamageInfo damage, CharacterStats stats) {
    var pipeline =
        WithCriticalMultiplier(2.0f)
        .Compose(WithArmorReduction(stats.Armor))
        .Compose(WithElementalResistance(DamageType.Fire, stats.FireResistance));

    return pipeline(damage);
}

// Extension for composition
public static Func<T, T> Compose<T>(this Func<T, T> first, Func<T, T> second) {
    return x => second(first(x));
}
```

---

## Antipatterns to Avoid

### No Primitive Obsession

```csharp
// ❌ WRONG - Primitives everywhere
public partial class Player : CharacterBody3D {
    public int Id { get; set; }
    public int TargetId { get; set; }
    public string State { get; set; }  // Magic string!
    public int Health { get; set; }  // No validation!
}

// ✅ CORRECT - Strongly typed
public partial class Player : CharacterBody3D {
    public PlayerId Id { get; private set; }
    public EntityId? TargetId { get; private set; }
    public CharacterState State { get; private set; }
    public Health Health { get; private set; }
}
```

### No Anaemic Entities

```csharp
// ❌ WRONG - Anaemic entity, logic scattered elsewhere
public partial class Enemy : CharacterBody3D {
    public int Health { get; set; }
    public bool IsDead { get; set; }
}

public class EnemyService {
    public void TakeDamage(Enemy enemy, int amount) {
        enemy.Health -= amount;
        if (enemy.Health <= 0) {
            enemy.IsDead = true;
        }
    }
}

// ✅ CORRECT - Rich entity with behaviour
public partial class Enemy : CharacterBody3D {
    [Signal] public delegate void DamageTakenEventHandler(int amount);
    [Signal] public delegate void DiedEventHandler();

    public Health Health { get; private set; }
    public bool IsDead => Health.IsDead;

    public Result TakeDamage(DamageInfo damage) {
        if (IsDead) {
            return Result.Fail("Already dead");
        }

        Health = Health.TakeDamage(damage.Amount);
        EmitSignal(SignalName.DamageTaken, damage.Amount);

        if (IsDead) {
            EmitSignal(SignalName.Died);
        }

        return Result.Ok();
    }
}
```

### No Fallback Mechanisms

```csharp
// ❌ WRONG - Hides real errors
public Node GetPlayer() {
    try {
        return GetNode("/root/Player");
    } catch {
        return null;  // Hides the real problem!
    }
}

// ✅ CORRECT - Surface errors properly
public Result<Player> GetPlayer() {
    var player = GetNodeOrNull<Player>("/root/Player");
    return player is null
        ? Result<Player>.NotFound()
        : Result<Player>.Ok(player);
}
```

### No Magic Strings

```csharp
// ❌ WRONG
if (state == "Attacking") { }
var scene = GD.Load<PackedScene>("res://scenes/enemy.tscn");
SetMeta("health", 100);

// ✅ CORRECT
if (state == CharacterState.Attacking) { }
var scene = GD.Load<PackedScene>(ScenePaths.Enemy);
SetMeta(MetaKeys.Health, 100);

// Define constants
public static class ScenePaths {
    public const string Enemy = "res://scenes/enemy.tscn";
    public const string Player = "res://scenes/player.tscn";
    public const string Projectile = "res://scenes/projectile.tscn";
}

public static class MetaKeys {
    public const string Health = "health";
    public const string Team = "team";
}
```

### No Deep Nesting (max 2 levels)

```csharp
// ❌ WRONG - Arrow code
public Result Process(AttackRequest r) {
    if (r != null) {
        if (r.IsValid) {
            if (r.Attacker.CanAttack) {
                if (r.Target != null) {
                    return ExecuteAttack(r);
                }
            }
        }
    }
    return Result.Fail("Invalid");
}

// ✅ CORRECT - Flat with early returns
public Result Process(AttackRequest r) {
    if (r is null) return Result.Fail("Null request");
    if (!r.IsValid) return Result.Fail("Invalid request");
    if (!r.Attacker.CanAttack) return Result.Fail("Cannot attack");
    if (r.Target is null) return Result.Fail("No target");

    return ExecuteAttack(r);
}
```

---

## Godot-Specific Patterns

### Node Lifecycle

```csharp
public partial class GameEntity : CharacterBody3D {
    // Called when node enters scene tree (before _Ready)
    public override void _EnterTree() {
        // Register with global systems
        GameManager.Instance.RegisterEntity(this);
    }

    // Called when node is ready (all children ready)
    public override void _Ready() {
        // Initialize - this is your "constructor" in Godot
        // Do NOT use C# constructors for node initialization
    }

    // Called every frame
    public override void _Process(double delta) {
        // Visual updates, non-physics logic
        UpdateAnimations(delta);
    }

    // Called every physics tick (fixed timestep)
    public override void _PhysicsProcess(double delta) {
        // Physics, movement, collision detection
        UpdateMovement(delta);
    }

    // Called when node exits scene tree
    public override void _ExitTree() {
        // Unregister from global systems
        GameManager.Instance.UnregisterEntity(this);
    }
}
```

### Godot Signals

```csharp
public partial class Player : CharacterBody3D {
    // Declare signals with delegate convention
    [Signal] public delegate void HealthChangedEventHandler(int current, int max);
    [Signal] public delegate void DiedEventHandler();
    [Signal] public delegate void AttackedEventHandler(EntityId targetId);

    // Emit signals
    public void TakeDamage(int amount) {
        _health -= amount;
        EmitSignal(SignalName.HealthChanged, _health, _maxHealth);

        if (_health <= 0) {
            EmitSignal(SignalName.Died);
        }
    }

    // Connect to signals in code
    public override void _Ready() {
        var enemy = GetNode<Enemy>("%Enemy");
        enemy.Died += OnEnemyDied;

        // Or use Connect for dynamic connections
        enemy.Connect(Enemy.SignalName.Died, Callable.From(OnEnemyDied));
    }

    private void OnEnemyDied() {
        GD.Print("Enemy defeated!");
    }
}
```

### Scene/Resource Loading

```csharp
public partial class SpawnManager : Node {
    // Preload scenes (loaded at compile time)
    private static readonly PackedScene EnemyScene =
        GD.Load<PackedScene>("res://scenes/enemies/enemy.tscn");
    private static readonly PackedScene ProjectileScene =
        GD.Load<PackedScene>("res://scenes/projectiles/arrow.tscn");

    // Export for editor configuration
    [Export] public PackedScene BossScene { get; set; }
    [Export] public float SpawnRadius { get; set; } = 10f;

    public Enemy SpawnEnemy(Vector3 position) {
        var enemy = EnemyScene.Instantiate<Enemy>();
        enemy.Position = position;
        GetTree().CurrentScene.AddChild(enemy);
        return enemy;
    }

    // Async scene loading for large scenes
    public async Task<Node> LoadSceneAsync(string path) {
        var loader = ResourceLoader.LoadThreadedRequest(path);

        while (ResourceLoader.LoadThreadedGetStatus(path) == ResourceLoader.ThreadLoadStatus.InProgress) {
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        }

        var scene = ResourceLoader.LoadThreadedGet(path) as PackedScene;
        return scene?.Instantiate();
    }
}
```

### Node References with Unique Names

```csharp
public partial class PlayerUI : Control {
    // Use %UniqueNames set in editor (Scene Unique Names)
    private Label _healthLabel;
    private ProgressBar _healthBar;
    private Label _ammoLabel;

    public override void _Ready() {
        // ✅ CORRECT - Using unique names (more resilient to hierarchy changes)
        _healthLabel = GetNode<Label>("%HealthLabel");
        _healthBar = GetNode<ProgressBar>("%HealthBar");
        _ammoLabel = GetNode<Label>("%AmmoLabel");

        // ❌ AVOID - Brittle path references
        // _healthLabel = GetNode<Label>("VBox/HBox/Panel/HealthLabel");
    }

    // Null-safe pattern for optional nodes
    public void UpdateScore(int score) {
        GetNodeOrNull<Label>("%ScoreLabel")?.SetText($"Score: {score}");
    }
}
```

### Node-Based State Machine

```csharp
public partial class CharacterStateMachine : Node {
    [Signal] public delegate void StateChangedEventHandler(
        CharacterState previous,
        CharacterState current);

    private static readonly FrozenDictionary<CharacterState, CharacterState[]> ValidTransitions =
        new Dictionary<CharacterState, CharacterState[]> {
            [CharacterState.Idle] = [CharacterState.Moving, CharacterState.Attacking, CharacterState.Dead],
            [CharacterState.Moving] = [CharacterState.Idle, CharacterState.Attacking, CharacterState.Dead],
            [CharacterState.Attacking] = [CharacterState.Idle, CharacterState.Moving, CharacterState.Dead],
            [CharacterState.Dead] = []
        }.ToFrozenDictionary();

    public CharacterState CurrentState { get; private set; } = CharacterState.Idle;

    public bool CanTransitionTo(CharacterState target) =>
        ValidTransitions[CurrentState].Contains(target);

    public Result TransitionTo(CharacterState target) {
        if (!CanTransitionTo(target)) {
            return Result.Fail($"Invalid transition: {CurrentState} → {target}");
        }

        var previous = CurrentState;
        CurrentState = target;
        EmitSignal(SignalName.StateChanged, (int)previous, (int)target);

        return Result.Ok();
    }
}

public enum CharacterState {
    Idle,
    Moving,
    Attacking,
    Dead
}
```

### GDScript Interop

```csharp
public partial class CSharpNode : Node {
    // Expose method to GDScript
    public void TakeDamage(int amount) {
        GD.Print($"Took {amount} damage from GDScript");
    }

    // Call GDScript method
    public void CallGDScriptMethod() {
        var gdNode = GetNode("/root/GDScriptNode");

        // Call method
        gdNode.Call("some_method", 42, "hello");

        // Call method and get return value
        var result = (int)gdNode.Call("get_value");

        // Access property
        var health = (int)gdNode.Get("health");
        gdNode.Set("health", 50);
    }

    // Handle Variant types from GDScript
    public void HandleVariant(Variant data) {
        // Check type and convert
        if (data.VariantType == Variant.Type.Int) {
            int value = data.AsInt32();
        } else if (data.VariantType == Variant.Type.String) {
            string text = data.AsString();
        } else if (data.VariantType == Variant.Type.Array) {
            var array = data.AsGodotArray();
            foreach (var item in array) {
                GD.Print(item);
            }
        }
    }

    // Expose signal that GDScript can connect to
    [Signal] public delegate void DataReadyEventHandler(Godot.Collections.Dictionary data);

    public void SendDataToGDScript() {
        var data = new Godot.Collections.Dictionary {
            ["player_id"] = 1,
            ["damage"] = 50,
            ["position"] = new Vector3(10, 0, 5)
        };
        EmitSignal(SignalName.DataReady, data);
    }
}
```

### Autoload Singleton Pattern

```csharp
// Register as Autoload in Project Settings
public partial class GameManager : Node {
    public static GameManager Instance { get; private set; }

    private readonly Dictionary<EntityId, GameEntity> _entities = new();

    public override void _Ready() {
        Instance = this;
    }

    public void RegisterEntity(GameEntity entity) {
        _entities[entity.Id] = entity;
    }

    public void UnregisterEntity(GameEntity entity) {
        _entities.Remove(entity.Id);
    }

    public GameEntity GetEntity(EntityId id) {
        return _entities.GetValueOrDefault(id);
    }

    public IEnumerable<T> GetEntitiesOfType<T>() where T : GameEntity {
        return _entities.Values.OfType<T>();
    }
}
```

---

## Logging and Error Handling

### Godot Logging

```csharp
// Use GD methods for Godot-integrated logging
public void ProcessAttack(AttackRequest request) {
    // Info level
    GD.Print($"Processing attack from {request.AttackerId} to {request.TargetId}");

    // Warning level (appears in yellow)
    if (request.Target is null) {
        GD.PushWarning($"Attack target {request.TargetId} not found");
    }

    // Error level (appears in red, with stack trace)
    if (request.Attacker is null) {
        GD.PushError($"Attack source {request.AttackerId} is null - this should never happen");
    }
}

// Structured logging with context
public void LogCombat(string action, EntityId source, EntityId target, int value) {
    GD.Print($"[Combat] {action}: {source} → {target} ({value})");
}
```

### Result for Expected, Exceptions for Unexpected

```csharp
public Result<Enemy> SpawnEnemy(SpawnRequest request) {
    // Validation failures are expected - use Result
    var validation = Validate(request);
    if (validation.IsFailure) {
        return Result<Enemy>.Fail(validation.Error);
    }

    // Check spawn position validity
    if (!IsValidSpawnPosition(request.Position)) {
        return Result<Enemy>.Fail("Invalid spawn position");
    }

    try {
        var enemy = _enemyScene.Instantiate<Enemy>();
        enemy.Position = request.Position;
        GetTree().CurrentScene.AddChild(enemy);

        GD.Print($"Spawned enemy at {request.Position}");
        return Result<Enemy>.Ok(enemy);
    } catch (Exception ex) {
        // Scene instantiation failure is unexpected
        GD.PushError($"Failed to spawn enemy: {ex.Message}");
        throw;
    }
}
```

---

## Testing Style

### Arrange-Act-Assert with Fluent Assertions

```csharp
[Fact]
public void TakeDamage_WhenAlive_ShouldReduceHealth() {
    // Arrange
    var health = Health.Create(100).Match(h => h, _ => throw new Exception());
    var damage = new DamageInfo(30, DamageType.Physical, EntityId.Empty);

    // Act
    var result = health.TakeDamage(damage.Amount);

    // Assert
    result.Current.Should().Be(70);
    result.IsDead.Should().BeFalse();
}

[Fact]
public void TakeDamage_WhenDamageExceedsHealth_ShouldDie() {
    // Arrange
    var health = Health.Create(50).Match(h => h, _ => throw new Exception());
    var damage = new DamageInfo(100, DamageType.Physical, EntityId.Empty);

    // Act
    var result = health.TakeDamage(damage.Amount);

    // Assert
    result.Current.Should().Be(0);
    result.IsDead.Should().BeTrue();
}

[Theory]
[InlineData(CharacterState.Dead)]
[InlineData(CharacterState.Attacking)]
public void TransitionTo_FromIdle_ToValidState_ShouldSucceed(CharacterState target) {
    // Arrange
    var stateMachine = new CharacterStateMachine();

    // Act
    var result = stateMachine.TransitionTo(target);

    // Assert
    result.IsSuccess.Should().BeTrue();
    stateMachine.CurrentState.Should().Be(target);
}

[Fact]
public void TransitionTo_FromDead_ShouldFail() {
    // Arrange
    var stateMachine = new CharacterStateMachine();
    stateMachine.TransitionTo(CharacterState.Dead);

    // Act
    var result = stateMachine.TransitionTo(CharacterState.Idle);

    // Assert
    result.IsFailure.Should().BeTrue();
    stateMachine.CurrentState.Should().Be(CharacterState.Dead);
}
```

---

## Quick Reference Checklist

When reviewing or writing code, check:

**C# Style:**
- [ ] Egyptian (cuddled) braces used?
- [ ] No deep nesting (max 2 levels)?
- [ ] Early returns for guard clauses?
- [ ] Strongly-typed IDs (not raw int/Guid)?
- [ ] No magic strings?
- [ ] Pattern matching where applicable?
- [ ] Switch expressions over switch statements?
- [ ] Records for DTOs and value objects?
- [ ] Rich entities (not anaemic)?
- [ ] Result pattern for expected failures?
- [ ] No XML documentation comments?
- [ ] No fallback mechanisms hiding errors?

**Godot-Specific:**
- [ ] Signals declared with `[Signal]` attribute?
- [ ] Node references using `%UniqueNames`?
- [ ] `_Ready()` for initialization, not constructor?
- [ ] Physics in `_PhysicsProcess`, not `_Process`?
- [ ] `CallDeferred()` for thread-safe node ops?
- [ ] `partial` keyword on all Node-derived classes?
- [ ] Scene paths as constants, not inline strings?
- [ ] Proper signal cleanup in `_ExitTree`?

---

*Document version: 2.0*
*Last updated: January 2025*
*For Godot 4.5.1 with C# / .NET 8*
*Friday Games Development Methodology*
