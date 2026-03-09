# QA Testing with godot-runtime

Automated QA via `mcp__godot-runtime` requires specific techniques. Read this before any QA session.

## Input Injection

| Method | Works? | Notes |
|---|---|---|
| `simulate_input` with `action` type | **NO** for `IsActionJustPressed` | Doesn't trigger polling in `_Process` |
| `simulate_input` with `mouse_motion` | **YES** | Camera look works fine |
| `simulate_input` with `move_*` actions | **YES** | WASD movement works (continuous press/release) |
| `run_script` + `InputEventKey` + `Input.parse_input_event()` | **YES** | Correct way for discrete actions |
| `run_script` + `await` | **NO** | Causes debugger break — MCP bridge doesn't support async |

### Sending Discrete Key Presses

Always use `run_script` with `InputEventKey`. Example for pressing E:

```gdscript
extends RefCounted

func execute(scene_tree: SceneTree) -> Variant:
    var kd = InputEventKey.new()
    kd.physical_keycode = KEY_E
    kd.keycode = KEY_E
    kd.pressed = true
    Input.parse_input_event(kd)
    var ku = InputEventKey.new()
    ku.physical_keycode = KEY_E
    ku.keycode = KEY_E
    ku.pressed = false
    Input.parse_input_event(ku)
    return "key_sent"
```

### Critical Rules

- **One action per `run_script` call** — `IsActionJustPressed()` only detects one press per frame. Batching multiple actions in one call silently drops all but the first.
- **Never use `await`** — causes "Trying to call an async function without await" debugger break.

## Player Positioning for Interaction

When testing raycasting interactions via MCP, you must position the player precisely:

1. Teleport player in front of the target node
2. Set rotation to face the target
3. Tilt camera down if the target is below eye level
4. Call `ray.force_raycast_update()` — required after teleporting (raycast doesn't update until next physics frame)
5. Verify `ray.is_colliding()` and `ray.get_collider().name` before sending input

## Common Pitfalls

- **Forgetting `force_raycast_update()`**: After teleporting, the raycast won't detect anything until the next physics frame unless you call this explicitly.
- **Camera not tilted**: If your ray origin is above the target, it will miss. Adjust camera rig X rotation (e.g., -0.4 to -0.5 radians).
- **Wrong facing direction**: Verify player rotation matches target position. Use `PI` to face +Z, `0` to face -Z.
- **Timeout during multi-step sequences**: Each `run_script` round-trip takes ~3-5s. For timed gameplay, increase timeout thresholds during QA.

## Recommended QA Workflow

```
1. run_project
2. take_screenshot — verify scene loaded
3. run_script — read game state
4. run_script — perform action (one per call)
5. take_screenshot — verify visual result
6. repeat steps 3-5 for each action in the test sequence
7. get_debug_output — check for errors
8. stop_project
```

## Design for MCP Testability

When writing game code that needs to be testable via MCP:

- Use `Input.IsActionJustPressed()` in `_Process()` for discrete actions (not `_UnhandledInput`)
- This ensures `Input.parse_input_event()` from `run_script` triggers the action
- Continuous movement can still use `Input.GetVector()` in `_PhysicsProcess()`
- Mouse look can stay in `_UnhandledInput` (receives `InputEventMouseMotion`)
