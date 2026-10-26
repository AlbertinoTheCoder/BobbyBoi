# Contributing to BobbyBoi

I welcome small fixes and new bad ideas for my desktop pet.

Small, focused changes are easier to review. Open an issue before starting a large new activity or renderer rewrite.

## Local setup

Use Windows and the .NET 10 SDK. Run `dotnet build -c Release` for development, or `build.bat` for the standalone EXE and regression checks.

Useful checks:

```powershell
dotnet run --project src/BobbyBoi -c Release -- --self-test
dotnet run --project src/BobbyBoi -c Release -- --sfx-test
dotnet run --project src/BobbyBoi -c Release -- --world-test
dotnet run --project src/BobbyBoi -c Release -- --chaos-test
dotnet run --project src/BobbyBoi -c Release -- --interruption-test
```

The first two are suitable for unattended Windows CI. World, menu, native-overlay and real-app delivery checks need a normal Windows desktop. Tests write to `artifacts/`, which is intentionally ignored.

## Keep the pet a good desktop neighbor

- Extend the existing activity director instead of starting competing movement loops.
- Keep cancellation, interruption/resume and cleanup paths working.
- Preserve existing settings and inventory when adding saved fields.
- Keep overlays lightweight and avoid stealing typing focus.
- Add menu entries only for working actions.
- Use original or appropriately licensed assets. Keep editable Blender sources with exported meshes.
- Keep file generation unique and avoid moving or closing pre-existing user windows.
- Do not add telemetry, paid services or secrets to the project.

## Blender assets

The scripts in `assets/` export JSON alongside their source files. For example:

```powershell
blender --background --factory-startup --python assets/build_chaos.py
```

Blender is an authoring dependency only. Runtime assets must remain embedded in the packaged EXE.

## Pull requests

Explain the visible behavior you changed and how you verified it. Include a small screenshot or clip for visual changes. Do not commit generated builds, personal settings, desktop screenshots, debug logs or test output.
