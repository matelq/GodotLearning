# Build & Project Structure

## What Goes Where

| Location | Content | Godot dependency |
|---|---|---|
| `scripts/` | Node classes, game scene logic | Yes — extends Node types |
| `src/{Name}/` | Domain logic, data models | None — pure C# |
| `tests/{Name}.Tests/` | xUnit v3 tests | References `src/` projects only |

## Adding a New C# Module

```bash
dotnet new classlib -n FirstProject.Networking -o src/FirstProject.Networking --no-restore
dotnet new xunit -n FirstProject.Networking.Tests -o tests/FirstProject.Networking.Tests --no-restore
dotnet sln FirstProject.slnx add src/FirstProject.Networking/FirstProject.Networking.csproj
dotnet sln FirstProject.slnx add tests/FirstProject.Networking.Tests/FirstProject.Networking.Tests.csproj
```

## Build Configuration

- `Directory.Build.props` — centralized TargetFramework (net10.0), Nullable, test packages, JetBrains.Annotations
- `Directory.Packages.props` — centralized package versions (CPM)
- `FirstProject.csproj` — must NOT define TargetFramework (inherits from Directory.Build.props)
- `GenerateAssemblyInfo=false` in FirstProject.csproj prevents CS0579 duplicates with Godot.NET.Sdk

## Git

**Commit:** `*.cs`, `*.tscn`, `*.tres`, `project.godot`, `*.slnx`, `*.csproj`, `Directory.Build.props`, `Directory.Packages.props`

**Never commit:** `.godot/`, `bin/`, `obj/`, `android/`
