# Godot 4.6 C# Patterns

Reference for common Godot patterns used in this project. Look up `godot-docs` MCP for authoritative API details.

## Node Hierarchy

Prefer this structure for player characters:
```
CharacterBody3D  (script.cs)
├── CollisionShape3D
├── MeshInstance3D
└── Node3D "CameraRig"
    └── Camera3D
```

## Body Types

- **CharacterBody3D** → player-controlled movement (`MoveAndSlide`)
- **RigidBody3D** → physics-driven objects (crates, projectiles)
- Never add physics forces to CharacterBody3D

## Input Handling

Discrete actions (interact, drop) use `Input.IsActionJustPressed()` in `_Process()` — required for MCP input injection compatibility. Continuous movement uses `Input.GetVector()` in `_PhysicsProcess()`.

## Signals

```csharp
// Define
[Signal] public delegate void HealthChangedEventHandler(int newHealth);
// Emit
EmitSignal(SignalName.HealthChanged, health);
// Connect
sourceNode.HealthChanged += OnHealthChanged;
```

## Scene Instancing

```csharp
[Export] public PackedScene EnemyScene { get; set; }
var enemy = EnemyScene.Instantiate<Enemy>();
GetTree().CurrentScene.AddChild(enemy);
```

## Autoloads

Access via: `GetNode<GameManager>("/root/GameManager")`

## Delta Time

- `_Process(double delta)` — every render frame (variable)
- `_PhysicsProcess(double delta)` — fixed rate (60 Hz with Jolt)
- Always cast: `(float)delta`
