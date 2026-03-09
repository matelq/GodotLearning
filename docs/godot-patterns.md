# Godot 4.6 C# Patterns

Reference for common Godot patterns. Look up `godot-docs` MCP for authoritative API details.

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

- **CharacterBody3D** — player-controlled movement (`MoveAndSlide`, `MoveAndCollide`)
- **RigidBody3D** — physics-driven objects (crates, projectiles, ragdolls)
- Never add physics forces to CharacterBody3D

## Input Handling

- Discrete actions: `Input.IsActionJustPressed()` in `_Process()`
- Continuous movement: `Input.GetVector()` in `_PhysicsProcess()`
- Mouse look: `InputEventMouseMotion` in `_UnhandledInput()`

## Signals

```csharp
// Define
[Signal] public delegate void HealthChangedEventHandler(int newHealth);
// Emit
EmitSignal(SignalName.HealthChanged, health);
// Connect (in _Ready of subscriber)
sourceNode.HealthChanged += OnHealthChanged;
```

## Scene Instancing

```csharp
[Export] public PackedScene EnemyScene { get; set; }
var enemy = EnemyScene.Instantiate<Enemy>();
GetTree().CurrentScene.AddChild(enemy);
```

## Autoloads (Singletons)

Register in Project Settings → Autoload. Access via:
```csharp
GetNode<MyManager>("/root/MyManager")
```

## Resources

```csharp
[Export] public MyResource Config { get; set; }                         // Inspector
ResourceLoader.Load<MyResource>("res://data/config.tres")              // Code
```

## Delta Time

- `_Process(double delta)` — every render frame (variable rate)
- `_PhysicsProcess(double delta)` — fixed rate (60 Hz with Jolt)
- Always cast: `(float)delta`

## Collision Layers

Use separate collision layers for selective blocking:
- Set `collision_layer` for what a body **is**
- Set `collision_mask` for what a body **collides with**
- Bodies only collide when one's layer matches the other's mask

## Programmatic Meshes

Build composed visuals from Godot primitives when prototyping:
```csharp
var mesh = new MeshInstance3D
{
    Mesh = new BoxMesh { Size = new Vector3(1, 1, 1) },
    MaterialOverride = new StandardMaterial3D { AlbedoColor = Colors.Red },
    Position = new Vector3(0, 0.5f, 0)
};
AddChild(mesh);
```

Supported primitives: `BoxMesh`, `SphereMesh`, `CylinderMesh`, `CapsuleMesh`, `PlaneMesh`.
