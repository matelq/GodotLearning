# CLAUDE.md — Godot 4.6 C# Project

## What This Is

A 3D game built with Godot 4.6 and C# (.NET 10).

## Tech Stack

| Property | Value |
|---|---|
| Engine | Godot 4.6 (Forward Plus, D3D12, Jolt Physics) |
| Language | C# only — no GDScript |
| Framework | .NET 10 |
| Solution | `FirstProject.slnx` |

## Project Layout

```
FirstProject.csproj        # Godot game (MUST stay at root)
scripts/                   # C# node classes (Godot dependency)
scenes/                    # .tscn scene files
ui/                        # UI scenes and scripts
src/{Name}/                # pure C# libraries (no Godot dependency)
tests/{Name}.Tests/        # xUnit v3 tests (reference src/ only)
docs/                      # detailed guides (read on demand)
```

## How to Build, Run, Test

```bash
dotnet build FirstProject.slnx        # build everything
dotnet test FirstProject.slnx         # run xUnit tests
```

Run the game via `godot-editor` MCP → `run_project`. Always check output for `ERROR:` or `SCRIPT ERROR:`.

## MCP Tools

You have 4 MCP servers. Use them, don't guess.

- **`godot-docs`** — Look up Godot class APIs BEFORE writing code that uses them. C# bindings differ from GDScript (PascalCase, `double delta`, constructor availability).
- **`serena`** — C# code intelligence. Use `find_symbol` before editing, use semantic edit tools for non-trivial changes.
- **`godot-editor`** — Run project, capture debug output, manage scenes. Run after every significant change.
- **`godot-runtime`** — Screenshots, input injection, live scene inspection. Read `docs/qa-testing.md` before any QA session.

## Critical Build Constraints

1. **Test projects must NOT reference `FirstProject.csproj`** — Godot.NET.Sdk's `ScriptPathAttributeGenerator` fails transitively. Put testable logic in `src/` libraries.
2. **`src/` and `tests/` are excluded** from Godot's `**/*.cs` glob via `<Compile Remove>` in `FirstProject.csproj`. Do not remove these excludes.
3. **TargetFramework is centralized** in `Directory.Build.props` — individual .csproj files must NOT define their own.

## C# / Godot Rules

Style is enforced by `.editorconfig` (build errors, not suggestions). Key points that the linter can't catch:

- All Godot node classes must be `partial class`
- `delta` is `double` in C# — cast with `(float)delta`
- Never use field initializers for Godot node refs — assign in `_Ready()`
- Private fields: `camelCase` with no underscore prefix
- Mark engine-invoked methods with `[UsedImplicitly]`

## Workflow

For non-trivial tasks: look up docs → find existing code → plan → write → run → check output → screenshot if visual → iterate.

## Detailed Guides

Read these on demand when the task requires it:

- **`docs/qa-testing.md`** — Runtime input injection, player positioning, QA workflow, common pitfalls
- **`docs/godot-patterns.md`** — Signals, scene instancing, input handling, node hierarchy patterns
- **`docs/build-and-structure.md`** — Adding new C# modules, what goes where, git guidelines
