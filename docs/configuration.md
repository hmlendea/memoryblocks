# Configuration

This document describes the configuration schema, sources, and precedence.

## Configuration Overview

Memory Blocks has **no external configuration system**. All configuration is compile-time constants embedded in the codebase. There are no configuration files, environment variables, command-line arguments, or runtime configuration APIs.

## Configuration Points

| Setting | Value | Location | Type |
|---------|-------|----------|------|
| Default Grid Size | 6 | `MainWindow` constructor, `Program.cs` | Compile-time constant |
| Timer Interval | 1000ms | `GameEngine` constructor | Compile-time constant |
| Background Color | `Color.GhostWhite` | `GameEngine` constructor | Compile-time constant |
| Primary Color (unflipped) | `Color.CornflowerBlue` | `GameEngine` constructor | Compile-time constant |
| Secondary Color (flipped) | `Color.LightBlue` | `GameEngine` constructor | Compile-time constant |
| Tile Spacing | 2px | `GameEngine.NewGame()` | Compile-time constant |
| Max Piece Value | 102 | `GameTable.piecesMaxValue` | Compile-time constant |
| Piece Size | 48 | `Piece` constructor | Compile-time constant (unused) |
| Easy Grid Size | 4 | `OnActEasyActivated` | Compile-time constant |
| Moderate Grid Size | 6 | `OnActModerateActivated` | Compile-time constant |
| Hard Grid Size | 8 | `OnActHardActivated` | Compile-time constant |
| Target Framework | .NET Framework 4.5 | `MemoryBlocks.csproj` | Build-time |
| Platform Target | x86 | `MemoryBlocks.csproj` | Build-time |
| Output Type | WinExe | `MemoryBlocks.csproj` | Build-time |

## Configuration Sources (Precedence Order)

1. **Source Code Constants** — Only source; no overrides possible
2. **Project File** — Build configuration (framework, platform, output type)
3. **Solution File** — Project configurations (Debug|x86, Release|x86)

## No Runtime Configuration

- No `app.config` / `web.config`
- No `appsettings.json`
- No environment variable reading
- No command-line argument parsing (args ignored in `Main`)
- No registry, no INI files, no user config directory
- No feature flags

## Changing Configuration

All configuration changes require **source code modification and recompilation**:

```bash
# Edit source files
# Rebuild
xbuild MemoryBlocks.sln
# Or
msbuild MemoryBlocks.sln
```

## Configuration-Related Risks

| Risk | Description |
|------|-------------|
| Asset/Value Mismatch | `piecesMaxValue = 102` but only 37 piece images (0–36) |
| Hardcoded Colours | No theming, no user preference, no accessibility options |
| Fixed Grid Sizes | Only 4×4, 6×6, 8×8 available |
| Fixed Timer Resolution | 1-second granularity only |
| No Localization | All strings hardcoded in English |

## Proposed Configuration Improvements

If configuration were added, recommended approach:

1. **Settings Class** — `GameSettings` with properties for all tunable values
2. **JSON File** — `gamesettings.json` in user config directory (`XDG_CONFIG_HOME`)
3. **Fallback Defaults** — Compile-time defaults used if file missing
4. **Validation** — Clamp grid size to even numbers 4–10, validate colour values
5. **Asset Validation** — Ensure `maxPieceValue <= assetCount` at startup

Example structure:
```json
{
  "defaultGridSize": 6,
  "timerIntervalMs": 1000,
  "colors": {
    "background": "GhostWhite",
    "primary": "CornflowerBlue",
    "secondary": "LightBlue"
  },
  "tileSpacing": 2,
  "maxPieceValue": 37,
  "difficultyLevels": {
    "easy": 4,
    "moderate": 6,
    "hard": 8
  }
}
```