# QA Helper Scripts for godot-runtime run_script
#
# These are copy-paste GDScript snippets for use with mcp__godot-runtime__run_script.
# Each snippet is a standalone script that extends RefCounted with an execute() method.
#
# IMPORTANT RULES for run_script:
#   1. Never use `await` — it causes a debugger break in the MCP bridge
#   2. Scripts must: extends RefCounted + func execute(scene_tree: SceneTree) -> Variant
#   3. Return data as Dictionary or String (gets JSON-serialized)
#   4. Use Input.parse_input_event(InputEventKey) for actions, NOT InputEventAction
#   5. Call ray.force_raycast_update() after repositioning before checking collisions


# ============================================================
# SNIPPET: get_game_state
# Returns full game state snapshot
# ============================================================
# extends RefCounted
#
# func execute(scene_tree: SceneTree) -> Variant:
#     var player = scene_tree.current_scene.get_node("Player")
#     var ray = player.get_node("CameraRig/InteractionRay")
#     ray.force_raycast_update()
#     var gm = scene_tree.root.get_node("GameManager")
#     var result = {
#         "player_pos": str(player.global_position),
#         "player_rotation": str(player.rotation),
#         "ray_colliding": ray.is_colliding(),
#         "ray_target": ray.get_collider().name if ray.is_colliding() else "none",
#         "game_running": gm.get("IsGameRunning"),
#         "score": gm.get("Score"),
#     }
#     # Read held ingredients from HUD
#     var hud = scene_tree.current_scene.get_node("HUD")
#     var held_slots = hud.get_node("HeldPanel/VBox/HeldSlots")
#     var held = []
#     for child in held_slots.get_children():
#         if child is HBoxContainer:
#             for sub in child.get_children():
#                 if sub is Label:
#                     held.append(sub.text.strip_edges())
#     result["held_ingredients"] = held
#     # Read current order from HUD
#     var order_slots = hud.get_node("OrderPanel/VBox/OrderSlots")
#     var order = []
#     for child in order_slots.get_children():
#         if child is HBoxContainer:
#             for sub in child.get_children():
#                 if sub is Label:
#                     order.append(sub.text.strip_edges())
#     result["order_ingredients"] = order
#     return result


# ============================================================
# SNIPPET: move_to_station
# Teleport player in front of a station and aim the ray at it.
# Replace STATION_NAME and STATION_POS with actual values.
#
# Station positions (from ShopLevel.tscn):
#   ChickenStation:     Vector3(-3, 0.5, 3.5)
#   TomatoStation:      Vector3(-1, 0.5, 3.5)
#   GarlicSauceStation: Vector3( 1, 0.5, 3.5)
#   FlatbreadStation:   Vector3( 3, 0.5, 3.5)
#   GrillStation:       Vector3( 5, 0.5, 3.5)
#   WrapStation:        Vector3(-4.5, 0.5, 0)
#   ServeCounter:       Vector3( 0, 0.5, -2.3)
# ============================================================
# extends RefCounted
#
# func execute(scene_tree: SceneTree) -> Variant:
#     var player = scene_tree.current_scene.get_node("Player")
#     var camera_rig = player.get_node("CameraRig")
#     var ray = player.get_node("CameraRig/InteractionRay")
#     # Stand 1.5m south of station, face north (+Z = PI rotation)
#     player.global_position = Vector3(-3, 0, 2.0)  # adjust X to match station
#     player.rotation = Vector3(0, PI, 0)
#     camera_rig.rotation = Vector3(-0.5, 0, 0)  # tilt down to hit station top
#     ray.force_raycast_update()
#     return {"aimed_at": ray.get_collider().name if ray.is_colliding() else "none"}


# ============================================================
# SNIPPET: send_interact
# Sends E key press via InputEventKey (works with Input.IsActionJustPressed)
# ============================================================
# extends RefCounted
#
# func execute(scene_tree: SceneTree) -> Variant:
#     var kd = InputEventKey.new()
#     kd.physical_keycode = KEY_E
#     kd.keycode = KEY_E
#     kd.pressed = true
#     Input.parse_input_event(kd)
#     var ku = InputEventKey.new()
#     ku.physical_keycode = KEY_E
#     ku.keycode = KEY_E
#     ku.pressed = false
#     Input.parse_input_event(ku)
#     return "interact_sent"


# ============================================================
# SNIPPET: send_drop
# Sends Q key press
# ============================================================
# extends RefCounted
#
# func execute(scene_tree: SceneTree) -> Variant:
#     var kd = InputEventKey.new()
#     kd.physical_keycode = KEY_Q
#     kd.keycode = KEY_Q
#     kd.pressed = true
#     Input.parse_input_event(kd)
#     var ku = InputEventKey.new()
#     ku.physical_keycode = KEY_Q
#     ku.keycode = KEY_Q
#     ku.pressed = false
#     Input.parse_input_event(ku)
#     return "drop_sent"


# ============================================================
# SNIPPET: pickup_at_station
# Combined: teleport to station, aim, press E — all in one call.
# Set station_x to the station's X position.
# For WrapStation/ServeCounter, adjust Z and rotation accordingly.
# ============================================================
# extends RefCounted
#
# func execute(scene_tree: SceneTree) -> Variant:
#     var station_x = -3.0  # ChickenStation=-3, Tomato=-1, Garlic=1, Flatbread=3
#     var player = scene_tree.current_scene.get_node("Player")
#     var camera_rig = player.get_node("CameraRig")
#     var ray = player.get_node("CameraRig/InteractionRay")
#     player.global_position = Vector3(station_x, 0, 2.0)
#     player.rotation = Vector3(0, PI, 0)
#     camera_rig.rotation = Vector3(-0.5, 0, 0)
#     ray.force_raycast_update()
#     var hit = ray.get_collider().name if ray.is_colliding() else "none"
#     # Send E key
#     var kd = InputEventKey.new()
#     kd.physical_keycode = KEY_E
#     kd.keycode = KEY_E
#     kd.pressed = true
#     Input.parse_input_event(kd)
#     var ku = InputEventKey.new()
#     ku.physical_keycode = KEY_E
#     ku.keycode = KEY_E
#     ku.pressed = false
#     Input.parse_input_event(ku)
#     return {"picked_from": hit}


# ============================================================
# SNIPPET: interact_at_wrap_station
# Teleport to wrap station, aim, press E
# ============================================================
# extends RefCounted
#
# func execute(scene_tree: SceneTree) -> Variant:
#     var player = scene_tree.current_scene.get_node("Player")
#     var camera_rig = player.get_node("CameraRig")
#     var ray = player.get_node("CameraRig/InteractionRay")
#     player.global_position = Vector3(-4.5, 0, -1.5)
#     player.rotation = Vector3(0, PI, 0)
#     camera_rig.rotation = Vector3(-0.4, 0, 0)
#     ray.force_raycast_update()
#     var hit = ray.get_collider().name if ray.is_colliding() else "none"
#     var kd = InputEventKey.new()
#     kd.physical_keycode = KEY_E
#     kd.keycode = KEY_E
#     kd.pressed = true
#     Input.parse_input_event(kd)
#     var ku = InputEventKey.new()
#     ku.physical_keycode = KEY_E
#     ku.keycode = KEY_E
#     ku.pressed = false
#     Input.parse_input_event(ku)
#     return {"wrapped_at": hit}


# ============================================================
# SNIPPET: interact_at_serve_counter
# Teleport to serve counter, aim, press E
# ============================================================
# extends RefCounted
#
# func execute(scene_tree: SceneTree) -> Variant:
#     var player = scene_tree.current_scene.get_node("Player")
#     var camera_rig = player.get_node("CameraRig")
#     var ray = player.get_node("CameraRig/InteractionRay")
#     player.global_position = Vector3(0, 0, -0.5)
#     player.rotation = Vector3(0, 0, 0)  # facing -Z (toward counter)
#     camera_rig.rotation = Vector3(-0.3, 0, 0)
#     ray.force_raycast_update()
#     var hit = ray.get_collider().name if ray.is_colliding() else "none"
#     var kd = InputEventKey.new()
#     kd.physical_keycode = KEY_E
#     kd.keycode = KEY_E
#     kd.pressed = true
#     Input.parse_input_event(kd)
#     var ku = InputEventKey.new()
#     ku.physical_keycode = KEY_E
#     ku.keycode = KEY_E
#     ku.pressed = false
#     Input.parse_input_event(ku)
#     return {"served_at": hit}


# ============================================================
# SNIPPET: full_serve_cycle
# WARNING: This does NOT work as a single run_script call!
# IsActionJustPressed() only detects one press per frame.
# Each pickup/wrap/serve MUST be a separate run_script call.
# This snippet is here as a REFERENCE for the sequence of steps.
# ============================================================
# extends RefCounted
#
# func execute(scene_tree: SceneTree) -> Variant:
#     var player = scene_tree.current_scene.get_node("Player")
#     var camera_rig = player.get_node("CameraRig")
#     var ray = player.get_node("CameraRig/InteractionRay")
#     var results = []
#
#     # Read order from HUD
#     var hud = scene_tree.current_scene.get_node("HUD")
#     var order_slots = hud.get_node("OrderPanel/VBox/OrderSlots")
#     var order = []
#     for child in order_slots.get_children():
#         if child is HBoxContainer:
#             for sub in child.get_children():
#                 if sub is Label:
#                     var txt = sub.text.strip_edges()
#                     if not txt.ends_with("[OK]"):
#                         order.append(txt)
#     results.append({"order": order})
#
#     # Station X positions by ingredient name
#     var station_x = {
#         "Chicken": -3.0, "Tomato": -1.0,
#         "Garlic Sauce": 1.0, "GarlicSauce": 1.0,
#         "Flatbread": 3.0
#     }
#
#     # Pick up each ingredient
#     for ingredient in order:
#         var x = station_x.get(ingredient, 0.0)
#         player.global_position = Vector3(x, 0, 2.0)
#         player.rotation = Vector3(0, PI, 0)
#         camera_rig.rotation = Vector3(-0.5, 0, 0)
#         ray.force_raycast_update()
#         var hit = ray.get_collider().name if ray.is_colliding() else "none"
#         _send_e()
#         results.append({"pickup": ingredient, "hit": hit})
#
#     # Wrap
#     player.global_position = Vector3(-4.5, 0, -1.5)
#     player.rotation = Vector3(0, PI, 0)
#     camera_rig.rotation = Vector3(-0.4, 0, 0)
#     ray.force_raycast_update()
#     _send_e()
#     results.append({"wrap": ray.get_collider().name if ray.is_colliding() else "none"})
#
#     # Serve
#     player.global_position = Vector3(0, 0, -0.5)
#     player.rotation = Vector3(0, 0, 0)
#     camera_rig.rotation = Vector3(-0.3, 0, 0)
#     ray.force_raycast_update()
#     _send_e()
#     results.append({"serve": ray.get_collider().name if ray.is_colliding() else "none"})
#
#     return results
#
# func _send_e():
#     var kd = InputEventKey.new()
#     kd.physical_keycode = KEY_E
#     kd.keycode = KEY_E
#     kd.pressed = true
#     Input.parse_input_event(kd)
#     var ku = InputEventKey.new()
#     ku.physical_keycode = KEY_E
#     ku.keycode = KEY_E
#     ku.pressed = false
#     Input.parse_input_event(ku)
