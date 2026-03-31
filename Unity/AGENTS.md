# AGENTS.md - Unity Game Project (Sunnyside Island)

This file provides guidelines for AI agents working in this Unity C# game codebase.

## Build/Lint/Test Commands

### Unity Testing
- Run all tests: Use Unity Test Runner via `Window > General > Test Runner`
- Run tests via CLI: `Unity -runTests -testResults <path> -testPlatform <platform>`
- Unity Test Framework package: `com.unity.test-framework` (NUnit-based)

### Build
- Build via Unity Editor: `File > Build Settings > Build`
- Build via CLI: `Unity -quit -batchmode -buildWindows64Player <path>`
- Target Platform: StandaloneWindows64 (primary)

### Code Quality
- Unity version: 6000.3.10f1
- C# Language Version: 9.0
- Target Framework: netstandard2.1
- Enable Roslyn analyzers: Microsoft.Unity.Analyzers enabled

## Dependency Injection (DI)

### Overview
This project uses a custom lightweight DI container (`DI.DIContainer`) for dependency management. Always use DI instead of `FindObjectOfType` or singleton patterns.

### DI Container Usage

```csharp
// Global registration (lives for entire game)
DIContainer.InitializeGlobal();
DIContainer.Global.Register<IPlayerService, PlayerService>();
DIContainer.Global.RegisterInstance<IGameConfig>(config);

// Scene-scoped container
public class GameSceneInstaller : Installer
{
    protected override void InstallBindings()
    {
        Bind<IEnemyManager, EnemyManager>();          // New instance per scene
        BindInstance<LevelData>(currentLevelData);    // Pre-created instance
    }
}
```

### Injection Patterns

```csharp
// Field injection (preferred for MonoBehaviour)
public class PlayerController : MonoBehaviour
{
    [Inject]
    private IInputService _inputService;
    
    [Inject("PlayerConfig")]
    private PlayerConfig _config;
}

// Constructor injection (preferred for plain C# classes)
public class InventoryService
{
    private readonly IItemRepository _repository;
    private readonly IEventBus _eventBus;
    
    public InventoryService(IItemRepository repository, IEventBus eventBus)
    {
        _repository = repository;
        _eventBus = eventBus;
    }
}

// Manual resolution (avoid when possible)
var service = DIContainer.Resolve<IPlayerService>();
```

### Registration Best Practices
- Register interfaces, not concrete types
- Use scene installers for scene-specific dependencies
- Register configurations as instances
- Avoid service locator pattern - prefer injection

## EventBus Pattern

### Overview
Use EventBus for loose coupling between systems. Events are plain C# classes.

```csharp
// Define event
public class PlayerLevelUpEvent
{
    public int NewLevel { get; set; }
    public int MaxHealth { get; set; }
}

// Subscribe
public class AchievementSystem : IInitializable
{
    private readonly IEventBus _eventBus;
    
    public AchievementSystem(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }
    
    public void Initialize()
    {
        _eventBus.Subscribe<PlayerLevelUpEvent>(OnPlayerLevelUp);
    }
    
    private void OnPlayerLevelUp(PlayerLevelUpEvent evt)
    {
        if (evt.NewLevel >= 10)
            UnlockAchievement("Level10");
    }
    
    public void Dispose()
    {
        _eventBus.Unsubscribe<PlayerLevelUpEvent>(OnPlayerLevelUp);
    }
}

// Publish
_eventBus.Publish(new PlayerLevelUpEvent { NewLevel = level, MaxHealth = maxHp });
```

### Event Guidelines
- Events are for notification, not command (fire and forget)
- Keep event classes simple (data only, no behavior)
- One event class per logical event type
- Always unsubscribe in `Dispose()` or `OnDestroy()`
- Use specific events rather than generic ones

## Architecture Principles

### SOLID

**Single Responsibility** - One class, one job
```csharp
// Good: PlayerMovement handles only movement
public class PlayerMovement : MonoBehaviour
{
    public void Move(Vector3 direction) { }
}

// Bad: Mixed responsibilities
public class Player : MonoBehaviour
{
    public void Move() { }
    public void Attack() { }
    public void SaveGame() { }  // Should be in SaveSystem
}
```

**Open/Closed** - Open for extension, closed for modification
```csharp
// Good: Strategy pattern
public interface IDamageCalculator { int Calculate(DamageContext ctx); }
public class PlayerDamageCalculator : IDamageCalculator { }
public class EnemyDamageCalculator : IDamageCalculator { }

// Add new calculator without modifying existing code
```

**Liskov Substitution** - Subtypes must be substitutable
```csharp
// Good: Interface segregation
public interface IMovable { void Move(Vector3 direction); }
public interface IAttackable { void Attack(IDamageable target); }

public class Enemy : MonoBehaviour, IMovable, IAttackable
```

**Interface Segregation** - Small, focused interfaces
```csharp
// Good: Split large interfaces
public interface IPlayerStats { int GetLevel(); }
public interface IPlayerInventory { void AddItem(ItemData item); }

// Bad: God interface
public interface IPlayer { /* 50 methods */ }
```

**Dependency Inversion** - Depend on abstractions
```csharp
// Good: Constructor injection with interface
public class QuestManager
{
    private readonly IQuestRepository _repository;
    public QuestManager(IQuestRepository repository) => _repository = repository;
}
```

### KISS (Keep It Simple, Stupid)
- Prefer simple solutions over clever ones
- Avoid premature abstraction
- If it takes more than 5 minutes to explain, it's too complex
- Use built-in Unity features before custom solutions
- One-liners are not always better

### YAGNI (You Aren't Gonna Need It)
- Don't implement features until actually needed
- Don't add "flexibility" that isn't required
- Don't optimize prematurely
- Start with the simplest implementation that works
- Refactor when requirements actually change

## Code Style Guidelines

### Formatting
- **Indentation**: 4 spaces (no tabs)
- **Braces**: Allman style (opening brace on new line)
- **Line endings**: CRLF (Windows)
- **Max line length**: 120 characters
- **Trailing whitespace**: Trimmed

### Naming Conventions
- **Classes/Structs/Enums**: PascalCase (e.g., `GameData`, `ItemType`)
- **Public members**: PascalCase (e.g., `ItemName`, `MaxStack`)
- **Private fields**: camelCase with underscore prefix (e.g., `_registrations`, `_container`)
- **Public fields**: camelCase for data classes (e.g., `itemId`, `baseValue`)
- **Methods**: PascalCase (e.g., `GetItem`, `Register`)
- **Local variables/parameters**: camelCase (e.g., `container`, `typeKey`)
- **Generic constraints**: PascalCase with T prefix (e.g., `TInterface`, `TImplementation`)
- **Constants**: PascalCase (e.g., `Global`)
- **Event classes**: Suffix with `Event` (e.g., `PlayerDiedEvent`)
- **Interface names**: Prefix with `I` (e.g., `IPlayerService`)

### Import Organization
Order using directives as follows:
1. System namespaces
2. Unity namespaces
3. Project namespaces (DI, EventBus, SunnysideIsland)
4. Aliased imports (if any)

```csharp
using System;
using System.Collections.Generic;
using UnityEngine;
using DI;
using EventBus;
```

### Type Declarations
- Always use explicit access modifiers (`public`, `private`, `protected`, `internal`)
- Prefer `var` when type is obvious from right side
- Use `readonly` for fields that don't change after initialization
- Use `static readonly` for static constants
- Mark classes as `sealed` unless designed for inheritance
- Interface methods: explicit implementation when ambiguous

### Error Handling
- Use specific exception types (e.g., `KeyNotFoundException`, `ArgumentException`)
- Custom exceptions should inherit from `Exception` with descriptive names
- Include contextual information in exception messages
- Use `Debug.LogWarning` for non-fatal issues, `Debug.LogError` for errors
- Implement `IDisposable` for classes managing unmanaged resources
- Fail fast: validate inputs at method entry

### Unity-Specific
- Use `[SerializeField]` to expose private fields to Inspector
- Use `[Header("=== Title ===")]` to group related fields in Inspector
- Use `[CreateAssetMenu]` for ScriptableObject assets
- Custom attributes: `[Inject]`, `[Column]`, `[Sheet]`, `[Ignore]`
- Follow Unity event method naming: `Awake()`, `Start()`, `Update()`, `OnDestroy()`
- Avoid `FindObjectOfType` - use DI instead
- Scene references: inject via Installer, not `GetComponent`

### Code Structure
- One public type per file (filename matches type name)
- Namespace hierarchy: `SunnysideIsland.*`, `DI`, `EventBus`, `ExcelConverter.*`
- Private helper methods below public methods
- Static members grouped together
- No `#region` directives
- Group by feature, not by type

```csharp
// Project structure example
Scripts/
  Player/
    PlayerController.cs
    PlayerMovement.cs
    PlayerConfig.cs
    IPlayerService.cs
  Inventory/
    InventoryManager.cs
    InventoryUI.cs
    ItemSlot.cs
```

### Documentation
- XML documentation for public APIs (required in framework code)
- Korean comments acceptable for internal/game-specific logic
- Use `/// <summary>` for class/method documentation
- Document exceptions thrown with `<exception cref="...">`
- Explain "why", not "what" (code shows what)

### Null Handling
- Use null-conditional operator (`?.`) where appropriate
- Check for null before accessing Unity objects (they can be "fake null")
- Use `TryGetValue` pattern for dictionary lookups
- Return early pattern preferred over nested if statements
- Null-check injected dependencies in constructor

```csharp
public class MyService
{
    private readonly IRepository _repository;
    
    public MyService(IRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }
}
```
