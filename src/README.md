# src/ — Additional C# Library Projects

This folder contains pure C# library projects (non-Godot) for complex subsystems.

## Pattern

Each module follows this structure:
```
src/FirstProject.{Name}/
	FirstProject.{Name}.csproj   ← Microsoft.NET.Sdk class library
tests/FirstProject.{Name}.Tests/
	FirstProject.{Name}.Tests.csproj  ← xUnit v3 test project
```

## Adding a new module

```bash
# 1. Create the project
mkdir src/FirstProject.Networking
dotnet new classlib -n FirstProject.Networking -o src/FirstProject.Networking --no-restore

# 2. Create its test project
mkdir tests/FirstProject.Networking.Tests
dotnet new xunit -n FirstProject.Networking.Tests -o tests/FirstProject.Networking.Tests --no-restore

# 3. Add both to the solution
dotnet sln FirstProject.slnx add src/FirstProject.Networking/FirstProject.Networking.csproj
dotnet sln FirstProject.slnx add tests/FirstProject.Networking.Tests/FirstProject.Networking.Tests.csproj

# 4. Add test → src reference
# Edit tests/FirstProject.Networking.Tests.csproj to add:
# <ProjectReference Include="..\..\src\FirstProject.Networking\FirstProject.Networking.csproj" />

# 5. Add src → game reference (when the game needs it)
# Edit FirstProject.csproj to add:
# <ProjectReference Include="src\FirstProject.Networking\FirstProject.Networking.csproj" />
```

## Rules
- `src/` projects must have ZERO dependency on Godot (no GodotSharp, no GD.* calls)
- Tests must work without Godot runtime
- Examples: networking protocols, game rules/logic, data models, AI, inventory systems
