# Integrations

This document describes external system integrations and interfaces.

## Current Integrations

### None

Memory Blocks has **zero external integrations**. It is a standalone desktop application with:
- No network communication
- No database connections
- No message queues
- No third-party APIs
- No plugin system
- No inter-process communication
- No file I/O (except reading embedded resources)

## Integration Points (Potential)

### 1. Operating System

| Integration | Current | Potential |
|-------------|---------|-----------|
| File associations | ❌ | `.mbgame` save files |
| Protocol handlers | ❌ | `memoryblocks://new-game?size=6` |
| Jump lists / recent files | ❌ | Recent games |
| Notifications | ❌ | Game complete toast |
| Auto-start | ❌ | Desktop entry |

### 2. Desktop Environment (Linux)

| Integration | Current | Potential |
|-------------|---------|-----------|
| `.desktop` file | ❌ | Application menu entry |
| AppStream metadata | ❌ | Software center listing |
| MIME types | ❌ | Save file association |
| D-Bus | ❌ | Remote control, status |
| System tray | ❌ | Minimize to tray |
| Screensaver inhibition | ❌ | Prevent sleep during game |

### 3. Windows

| Integration | Current | Potential |
|-------------|---------|-----------|
| Start Menu shortcut | ❌ | Installer creates |
| File Explorer preview | ❌ | Thumbnail for save files |
| Taskbar jump list | ❌ | Recent difficulties |
| Toast notifications | ❌ | Game complete |
| Windows Registry | ❌ | Settings persistence |

### 4. Cloud Services (Not Applicable)

| Service | Applicability |
|---------|---------------|
| Leaderboards | ❌ No network |
| Cloud saves | ❌ No network |
| Analytics | ❌ No network |
| Crash reporting | ❌ No network |
| Auto-update | ❌ No network |

### 5. Accessibility

| Integration | Current | Potential |
|-------------|---------|-----------|
| ATK/AT-SPI | ⚠️ Indirect (GTK#) | Explicit labels, roles |
| Screen readers | ❌ Not tested | Orca, NVDA support |
| High contrast | ❌ Hardcoded colours | Theme-aware colours |
| Keyboard navigation | ⚠️ Partial (menu only) | Full keyboard play |
| Focus management | ⚠️ Basic | Logical tab order |

### 6. Internationalisation

| Integration | Current | Potential |
|-------------|---------|-----------|
| gettext | ❌ | `.po` files, `bindtextdomain` |
| Locale detection | ❌ | `CultureInfo.CurrentUICulture` |
| RTL support | ❌ | GTK handles if enabled |
| Number formatting | ❌ | Timer uses invariant format |

### 7. Hardware

| Integration | Current | Potential |
|-------------|---------|-----------|
| Touchscreen | ⚠️ Works (click = tap) | Touch-optimised UI |
| Gamepad | ❌ | Navigation, flip action |
| Keyboard | ⚠️ Menu only | Arrow keys + Space/Enter |

## Embedded Resource Integration

The only "integration" is with the assembly's own embedded resources:

```csharp
// GameEngine.DrawTable
Gdk.Pixbuf.LoadFromResource(
    "MemoryBlocks.Resources.Pieces.piece" + piece.Value + ".png")
```

**Resource Naming Convention:**
- Namespace: `MemoryBlocks.Resources.Pieces`
- Pattern: `piece{value}.png`
- Values: 0–36 (37 images)
- Format: PNG, 48×48 pixels (assumed)

**Resource Manifest:**
```
MemoryBlocks.Resources.Pieces.piece0.png
MemoryBlocks.Resources.Pieces.piece1.png
...
MemoryBlocks.Resources.Pieces.piece36.png
MemoryBlocks.gtk-gui.gui.stetic
MemoryBlocks.icon.png
```

## Build-Time Integrations

| Tool | Purpose | Integration Type |
|------|---------|------------------|
| `xbuild` / `msbuild` | Compilation | Build system |
| `pkg-config` | GTK# discovery | Build dependency |
| `monodis` | Resource inspection | Debugging |
| `mcs` / `csc` | C# compiler | Language toolchain |

## No Plugin Architecture

| Aspect | Status |
|--------|--------|
| Plugin interface | ❌ None |
| Dynamic loading | ❌ None |
| Scripting | ❌ None |
| Mod support | ❌ None |
| Theme engine | ❌ Hardcoded |

## Integration Testing

No integration tests exist. If integrations added:

| Integration | Test Approach |
|-------------|---------------|
| File association | `xdg-mime query filetype` / Windows `assoc` |
| Desktop entry | `desktop-file-validate` |
| D-Bus | `dbus-send` / `gdbus` |
| Accessibility | `accerciser`, screen reader testing |
| Internationalisation | `LANG=de_DE ./MemoryBlocks` |

## Future Integration Considerations

### If Network Added
- HTTPS only, certificate validation
- No telemetry without consent
- Local-first, sync optional

### If Persistence Added
- JSON in `XDG_DATA_HOME` / `%LOCALAPPDATA%`
- Versioned schema with migration
- No external database dependency

### If Plugin System Added
- MEF or custom `IPlugin` interface
- Sandboxed (no file/net access)
- Signed plugins only

## Summary

| Category | Count | Status |
|----------|-------|--------|
| Network | 0 | N/A |
| Database | 0 | N/A |
| IPC | 0 | N/A |
| OS/Desktop | 0 | Missing standard integrations |
| Accessibility | 1 (GTK) | Partial |
| i18n | 0 | Missing |
| Hardware | 1 (mouse) | Basic |
| Plugins | 0 | N/A |

**Recommendation:** Add `.desktop` file and basic accessibility labels as first integrations.