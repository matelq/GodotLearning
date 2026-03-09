# Build & Project Structure

## What Goes Where

| Location | Content | Godot dependency |
|---|---|---|
| `scripts/` | Node classes, game scene logic | Yes — extends Node types |
| `src/{Name}/` | Domain logic, data models, protocols | None — pure C# |
| `tests/{Name}.Tests/` | xUnit v3 tests | References `src/` projects only |

## Adding a New C# Module

```bash
dotnet new classlib -n FirstProject.Networking -o src/FirstProject.Networking --no-restore
dotnet new xunit -n FirstProject.Networking.Tests -o tests/FirstProject.Networking.Tests --no-restore
dotnet sln FirstProject.slnx add src/FirstProject.Networking/FirstProject.Networking.csproj
dotnet sln FirstProject.slnx add tests/FirstProject.Networking.Tests/FirstProject.Networking.Tests.csproj
```

## Build Configuration

- **`Directory.Build.props`** — centralized TargetFramework (net10.0), Nullable, TreatWarningsAsErrors, test packages, JetBrains.Annotations
- **`Directory.Packages.props`** — centralized package versions (CPM)
- **`FirstProject.csproj`** — must NOT define TargetFramework (inherits from Directory.Build.props)
- `GenerateAssemblyInfo=false` in FirstProject.csproj prevents CS0579 duplicates with Godot.NET.Sdk

## Key Constraints

1. **Test projects must NOT reference `FirstProject.csproj`** — Godot.NET.Sdk's `ScriptPathAttributeGenerator` fails transitively in non-Godot projects. Extract testable logic into `src/` libraries.
2. **`src/` and `tests/` are excluded** from Godot's `**/*.cs` glob via `<Compile Remove>`. Removing these causes build failures from duplicate/incompatible source inclusion.
3. **CSG classes** (`CSGBox3D`, etc.) may not be available in CLI builds. Use `MeshInstance3D` + primitive meshes instead for programmatic geometry.

## Git

**Commit:** `*.cs`, `*.tscn`, `*.tres`, `project.godot`, `*.slnx`, `*.csproj`, `Directory.Build.props`, `Directory.Packages.props`

**Never commit:** `.godot/`, `bin/`, `obj/`, `android/`
