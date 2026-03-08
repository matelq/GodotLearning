# CLAUDE.md — AI Assistant Guidelines for FirstProject

## Project Overview

| Property | Value |
|---|---|
| Engine | Godot 4.6 (Forward Plus, D3D12 on Windows) |
| Physics | Jolt Physics |
| Language | C# only — no GDScript |
| Framework | .NET 10 |
| Game type | 3D |
| Solution | `FirstProject.slnx` |

---

## MCP Tools — Mandatory Usage Rules

You have 4 MCP servers. Use them, don't guess.

### `godot-docs` — Godot API reference
**RULE: Call `godot_docs_get_class` BEFORE writing code that uses any Godot class.**

This is non-negotiable. Godot 4's C# API has subtle differences from GDScript docs. Always verify:
- Method signatures (especially `delta` type is `double`, not `float`)
- Property names (C# uses PascalCase, GDScript uses snake_case)
- Which methods exist on a class vs its parent
- Constructor availability (many Godot objects cannot be `new`'d directly)

```
godot_docs_get_class("Node3D")
godot_docs_get_class("CharacterBody3D")
godot_docs_search("RigidBody3D move")
```

### `serena` — C# code intelligence
**RULE: Use Serena for ALL C# edits beyond trivial one-liners.**

Serena understands your solution's symbol graph. Use it to:
- Find class/method definitions before editing (`find_symbol`)
- Make safe semantic edits that understand context
- Navigate the codebase without guessing file paths
- Avoid introducing typos in class names or method signatures

### `godot-editor` — Editor and project control
**RULE: Run project and capture output after every significant change.**

Use it to:
- Launch/run the project in debug mode
- Capture all console output (GD.Print, errors, warnings)
- List project structure and scene files
- Create and modify scenes without opening the editor

Always check captured output for `ERROR:` or `SCRIPT ERROR:` lines before proceeding.

### `godot-runtime` — Live game testing
**RULE: Use for visual QA and bug reproduction.**

The runtime MCP injects a UDP bridge when the game runs. Use it to:
- Take viewport screenshots to verify rendering
- Simulate player input (keyboard, mouse, UI clicks)
- Inspect the live scene tree while the game is running
- Execute GDScript snippets against the running SceneTree
- Reproduce reported bugs by replaying input sequences

---

## Mandatory Workflow

For every non-trivial task, follow this sequence:

```
1. DOCS    → godot_docs_get_class for any Godot types involved
2. SEARCH  → serena find_symbol to locate existing code
3. PLAN    → think through the implementation before touching files
4. WRITE   → make the code change
5. RUN     → godot-editor: run project, capture output
6. CHECK   → read all output for errors
7. VISUAL  → godot-runtime: screenshot if anything renders
8. ITERATE → fix errors, re-run
```

Never skip steps 1–2 for code involving Godot APIs.

---

## C# Style Guide

Sourced from `.editorconfig` — these are **build errors**, not suggestions.

### Naming
| Symbol | Convention | Example |
|---|---|---|
| Class | PascalCase | `PlayerController` |
| Property | PascalCase | `MoveSpeed` |
| Static/const field | PascalCase | `DefaultGravity` |
| Private field | camelCase (no prefix) | `moveSpeed`, `inputDir` |
| Method | PascalCase | `HandleInput()` |
| Local variable | camelCase | `var deltaPos` |

**No underscore prefixes on private fields.** `camelCase` only.
**No `this.` qualifier.** Omit it always.

### Code style
- Max line length: **150 chars**
- Braces: **always required** (even single-line `if`) — Allman style (brace on own line)
- `var` when type is apparent from context
- Prefer `null` propagation (`?.`) over null checks
- Prefer expression-bodied properties (`=> value`)
- Always `using` with braces for disposables

### C# / Godot specifics
- All Godot node classes must be `partial class`
- `delta` parameter type is `double` in C#, not `float` — cast or use `(float)delta`
- Never use field initializers for Godot node refs — assign in `_Ready()`
- Mark engine-invoked methods with `[UsedImplicitly]` to suppress Rider warnings

```csharp
public partial class PlayerController : CharacterBody3D
{
    [Export] public float MoveSpeed { get; set; } = 5.0f;

    [Signal] public delegate void PlayerDiedEventHandler();

    private Node3D cameraRig;  // camelCase, no underscore
    private float currentSpeed;

    [UsedImplicitly]
    public override void _Ready()
    {
        cameraRig = GetNode<Node3D>("CameraRig");
    }

    [UsedImplicitly]
    public override void _PhysicsProcess(double delta)
    {
        var velocity = Velocity;
        velocity.Y -= 9.8f * (float)delta;
        Velocity = velocity;
        MoveAndSlide();
    }
}
```

---

## Godot 3D Patterns

### Node hierarchy — prefer this structure for player
```
CharacterBody3D  (PlayerController.cs)
├── CollisionShape3D
├── MeshInstance3D
└── Node3D "CameraRig"
    └── Camera3D
```

### CharacterBody3D vs RigidBody3D
- **CharacterBody3D** → player-controlled movement (`MoveAndSlide`, `MoveAndCollide`)
- **RigidBody3D** → physics-driven objects (crates, projectiles, ragdolls)
- Never mix: don't add physics forces to CharacterBody3D manually

### Input handling
```csharp
// In _PhysicsProcess — use Input singleton
var inputDir = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
var direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
```

### Signals (inter-node communication)
```csharp
// Define
[Signal] public delegate void HealthChangedEventHandler(int newHealth);

// Emit
EmitSignal(SignalName.HealthChanged, health);

// Connect (in _Ready of subscriber)
sourceNode.HealthChanged += OnHealthChanged;
```

### Resources (shared data)
- Create `.tres` resource files for materials, game config, item definitions
- Use `[Export] public MyResource Config { get; set; }` to expose in Inspector
- Load in code: `ResourceLoader.Load<MyResource>("res://data/config.tres")`

### Autoloads (singletons)
- Register in Project → Project Settings → Autoload
- Access anywhere: `GetNode<GameManager>("/root/GameManager")`
- Use for: game state, audio manager, event bus, scene transitions

### Scene instancing
```csharp
// Preload (compile-time, for small scenes)
[Export] public PackedScene BulletScene { get; set; }

// Instantiate
var bullet = BulletScene.Instantiate<Bullet>();
GetTree().CurrentScene.AddChild(bullet);
bullet.GlobalPosition = muzzlePoint.GlobalPosition;
```

### Delta time
- `_Process(double delta)` — called every render frame (variable rate)
- `_PhysicsProcess(double delta)` — called at fixed rate (60 Hz default with Jolt)
- Always multiply by `(float)delta` for frame-rate independent movement

---

## Solution & Project Structure

```
first-project/
├── FirstProject.csproj          ← Godot game (MUST stay at root — Godot SDK requirement)
├── FirstProject.slnx            ← solution
├── Directory.Build.props        ← shared: TargetFramework, Nullable, test packages
├── Directory.Packages.props     ← centralized package versions (CPM)
├── scripts/                     ← C# game scripts (compiled by FirstProject.csproj)
├── scenes/                      ← .tscn scene files
├── assets/textures|models|audio ← game assets
├── ui/                          ← UI scenes and scripts
├── addons/                      ← Godot plugins
├── src/{Name}/                  ← pure C# library projects (no Godot dependency)
│   └── {Name}.csproj
└── tests/{Name}.Tests/          ← xUnit v3 test projects
    └── {Name}.Tests.csproj
```

### Adding a new C# module
```bash
dotnet new classlib -n FirstProject.Networking -o src/FirstProject.Networking --no-restore
dotnet new xunit -n FirstProject.Networking.Tests -o tests/FirstProject.Networking.Tests --no-restore
dotnet sln FirstProject.slnx add src/FirstProject.Networking/FirstProject.Networking.csproj
dotnet sln FirstProject.slnx add tests/FirstProject.Networking.Tests/FirstProject.Networking.Tests.csproj
```

### What belongs in `src/` vs `scripts/`
| Location | Content | Godot dependency |
|---|---|---|
| `scripts/` | Node classes, game scenes logic | Yes — extends Node types |
| `src/FeatureName/` | Domain logic, protocols, data models | None — pure C# |
| `tests/` | xUnit v3 tests | References `src/` projects only |

**CRITICAL: Test projects must NOT reference `FirstProject.csproj` directly.**
Godot.NET.Sdk's `ScriptPathAttributeGenerator` fails transitively in non-Godot projects.
Test pure C# logic by putting it in `src/` libraries and testing those instead.

**`src/` and `tests/` are excluded from the Godot project's `**/*.cs` glob** (via `<Compile Remove>` in `FirstProject.csproj`). Do not remove these excludes.

---

## Debug Workflow

1. Run project via `godot-editor` MCP
2. Always read full captured output — search for `ERROR:` and `SCRIPT ERROR:`
3. For visual bugs: take screenshot via `godot-runtime` MCP
4. For logic bugs: use live GDScript execution via `godot-runtime` to inspect state
5. Common Godot C# errors:
   - `Invalid call to method` → wrong method name (check docs with `godot_docs_get_class`)
   - `Cannot cast` → node type mismatch (use `GetNode<CorrectType>`)
   - `Null instance` → node path wrong or `_Ready()` didn't run yet
   - CS8618 (nullable) → suppressed by `Directory.Build.props` for Godot patterns

---

## Git Guidelines

**Commit these:**
- `*.cs` scripts
- `*.tscn` scene files
- `*.tres` resource files
- `project.godot`
- `FirstProject.slnx`
- `Directory.Build.props`, `Directory.Packages.props`
- `*.csproj` files

**Never commit:**
- `.godot/` directory (editor cache — gitignored)
- `android/` directory
- Build output (`bin/`, `obj/`)

---

## Learning Notes

- Build small isolated **test scenes** for each new feature before integrating
- Prefer **composition** (child nodes, components) over deep inheritance
- When stuck on a Godot API: look up docs first (`godot_docs_get_class`), then test in a scratch scene
- Use `GD.Print()` liberally — capture it with `godot-editor` MCP instead of guessing
- Annotate experimental code with `// TODO:` or `// EXPERIMENT:` to track learning progress
- When an approach isn't working after 2 attempts, stop and re-read the Godot docs for that class
