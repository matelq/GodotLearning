# QA Testing with godot-runtime

Automated QA via `mcp__godot-runtime` requires specific techniques.

## Input Injection

| Method | Works? | Notes |
|---|---|---|
| `simulate_input` with `action` type | **NO** for `IsActionJustPressed` | Doesn't trigger polling in `_Process` |
| `simulate_input` with `mouse_motion` | **YES** | Camera look works fine |
| `simulate_input` with `move_*` actions | **YES** | WASD movement works |
| `run_script` + `InputEventKey` + `Input.parse_input_event()` | **YES** | Correct way for discrete actions (E, Q) |
| `run_script` + `await` | **NO** | Causes debugger break. Never use `await` in run_script |

### Sending Key Presses

Use `run_script` with `InputEventKey` for discrete actions. See `qa/qa_helpers.gd` for copy-paste snippets.

One action per `run_script` call — `IsActionJustPressed` only detects one press per frame.

## Player Positioning

The RayCast3D is at camera height (Y=1.6) pointing forward 2.5m. Stations are at Y=0.5.

1. Teleport player 1.5m in front of station
2. Set `player.rotation = Vector3(0, PI, 0)` to face +Z (back-wall stations) or `(0, 0, 0)` for serve counter
3. Set `camera_rig.rotation = Vector3(-0.5, 0, 0)` to tilt down
4. Call `ray.force_raycast_update()` — required after repositioning
5. Verify `ray.is_colliding()` and `ray.get_collider().name` before sending input

## Station Positions (ShopLevel.tscn)

| Station | Position | Stand At | Facing |
|---|---|---|---|
| ChickenStation | `(-3, 0.5, 3.5)` | `(-3, 0, 2.0)` | +Z (`PI`) |
| TomatoStation | `(-1, 0.5, 3.5)` | `(-1, 0, 2.0)` | +Z |
| GarlicSauceStation | `(1, 0.5, 3.5)` | `(1, 0, 2.0)` | +Z |
| FlatbreadStation | `(3, 0.5, 3.5)` | `(3, 0, 2.0)` | +Z |
| GrillStation | `(5, 0.5, 3.5)` | `(5, 0, 2.0)` | +Z |
| WrapStation | `(-4.5, 0.5, 0)` | `(-4.5, 0, -1.5)` | +Z |
| ServeCounter | `(0, 0.5, -2.3)` | `(0, 0, -0.5)` | -Z (`0`) |

## QA Helper Snippets

Copy-paste snippets in `qa/qa_helpers.gd`: `get_game_state`, `pickup_at_station`, `interact_at_wrap_station`, `interact_at_serve_counter`, `full_serve_cycle`.

## Recommended Workflow

```
1. run_project
2. take_screenshot — verify scene loaded
3. run_script(get_game_state) — read current order
4. run_script(pickup_at_station) — for each ingredient
5. take_screenshot — verify HOLDING panel
6. run_script(interact_at_wrap_station)
7. take_screenshot — verify "SHAWARMA READY"
8. run_script(interact_at_serve_counter)
9. run_script(get_game_state) — verify score
10. get_debug_output — check for errors
11. stop_project
```

## Common Pitfalls

- **Camera not tilted**: Ray at Y=1.6 goes above stations (top at Y=1.0). Always tilt to -0.4 or -0.5.
- **Forgetting `force_raycast_update()`**: Raycast doesn't update until next physics frame after teleport.
- **Multiple actions in one `run_script`**: Only the first registers. One call per action.
- **Customer patience timeout**: Each round-trip ~3-5s. Set `CustomerPatienceSeconds >= 60` for QA.
- **Player facing wrong direction**: Back-wall = `PI`, serve counter = `0`.
